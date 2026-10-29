namespace FiapCloudGames.Orders.Application.Features.PurchaseOrders.Create;

public sealed class CreatePurchaseOrderService(
    IPurchaseOrderRepository purchaseOrderRepository,
    ICatalogGateway catalogGateway,
    IPromotionsGateway promotionsGateway,
    ILibraryGateway libraryGateway,
    IOrdersUnitOfWork unitOfWork,
    IIntegrationEventPublisher events,
    TimeProvider clock,
    ILogger<CreatePurchaseOrderService> logger)
{
    public async Task<PurchaseOrderResult> ExecuteAsync(
        CreatePurchaseOrderInput input,
        CancellationToken cancellationToken)
    {
        // OBS: Se alguma das validações falhar não processa a compra, processo atômico, ou faz tudo ou faz nada.

        var items = input.items.ToArray();
        var errors = GetDuplicateItemErrors(items).ToList();

        if (items.Length == 0)
            errors.Add(new AppError(
                "A ordem de compra deve possuir pelo menos um produto.",
                "items"));

        var activeGamesTask = purchaseOrderRepository.GetGamesInActiveOrdersAsync(
            input.CustomerId,
            items.Select(item => item.ProductId).Distinct().ToArray(),
            cancellationToken);
        var itemValidationsTask = Task.WhenAll(
            items.Select((item, index) => ValidateItemAsync(
                input.CustomerId,
                item,
                index,
                cancellationToken)));

        await Task.WhenAll(activeGamesTask, itemValidationsTask);

        var activeGames = (await activeGamesTask).ToHashSet();
        errors.AddRange(items
            .Select((item, index) => new { Item = item, Index = index })
            .Where(entry => activeGames.Contains(entry.Item.ProductId))
            .Select(entry => new AppError(
                $"O produto {entry.Item.ProductId} já está associado a uma ordem em andamento.",
                $"items[{entry.Index}].productId")));

        var itemValidations = await itemValidationsTask;

        errors.AddRange(itemValidations.SelectMany(result => result.Errors));

        if (errors.Count > 0)
            throw new BusinessRuleValidationException(errors);

        var purchaseOrderItems = itemValidations
            .Select(result => result.Item!)
            .ToArray();

        // Cria o agregado somente após todas as regras de negócio serem atendidas.
        var purchaseOrder = PurchaseOrder.Create(input.CustomerId, purchaseOrderItems, clock.GetUtcNow());

        // Workaround para vincular o número da ordem da compra a cada item da compra.
        foreach (var item in purchaseOrder.Items)
        {
            logger.LogInformation("Vinculando o produto {ProductId} à ordem de compra {Id}", item.ProductId, purchaseOrder.Id);
            item.BindItemsToPurchaseOrder(purchaseOrder.Id);
        }

        logger.LogInformation("Gerando ordem de compra.");

        // Adiciona a ordem de compra à unidade de trabalho.
        await purchaseOrderRepository.AddAsync(purchaseOrder, cancellationToken);
        await purchaseOrderRepository.AddReservationsAsync(
            purchaseOrder.Items.Select(item => PurchaseOrderGameReservation.Create(
                purchaseOrder.Id,
                purchaseOrder.CustomerId,
                item.ProductId,
                purchaseOrder.CreatedAtUtc)),
            cancellationToken);

        // OBS: Order é o orquestrador de tudo.

        // Publica o evento indicando que uma ordem de compra foi recebida.
        await events.PublishAsync(new PurchaseOrderCreatedEvent(Guid.NewGuid(), purchaseOrder.Id), cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // TODO: 2 - order worker (consumer 1 para enviar p/ pagto) fica ouvindo a mensagem publicada no RabbitMQ , quando consumir, dispara uma mensagem para criar um pagamento.
        // TODO: 3 - order worker (consumer 2 para receber do pagto se sucesso ou não).

        logger.LogInformation("Ordem de compra criada em {CreatedAt} as {CreatedAtHour}.",
            purchaseOrder.CreatedAtUtc.ToString("dd/MM/yyyy"),
            purchaseOrder.CreatedAtUtc.ToString("HH:mm:ss.fff"));

        return OrdersApplicationMappings.ToResult(purchaseOrder);

        // OBS: toda vez que algum ms bate no Order é um consumer.

        // TODO: depois de tudo, um consumer 3 do worker deverá enviar uma mensagem para o Library pra vincular o usário x List<Games>;
    }

    private async Task<ItemValidationResult> ValidateItemAsync(
        Guid customerId,
        CreatePurchaseOrderItemsInput item,
        int index,
        CancellationToken cancellationToken)
    {
        var errors = new List<AppError>();
        var productField = $"items[{index}].productId";
        var promotionField = $"items[{index}].promotionId";

        var libraryTask = libraryGateway.CheckGameExistsOnLibraryAsync(
            customerId,
            item.ProductId,
            cancellationToken);
        var gameTask = catalogGateway.GetGameByIdAsync(
            item.ProductId,
            cancellationToken);
        var promotionTask = item.PromotionId is not null
            ? promotionsGateway.GetByIdAsync(
                item.PromotionId.Value,
                cancellationToken)
            : null;

        var pendingTasks = new List<Task> { libraryTask, gameTask };
        if (promotionTask is not null)
            pendingTasks.Add(promotionTask);

        await Task.WhenAll(pendingTasks);

        var library = GetSuccessfulResponse(
            await libraryTask,
            "Library");
        if (library.Exists)
            errors.Add(new AppError(
                $"O cliente {customerId} já possui o produto {item.ProductId}.",
                productField));

        var game = ResolveGame(
            item,
            productField,
            await gameTask,
            errors);
        if (game is { IsActive: false })
            errors.Add(new AppError(
                $"O jogo ID {item.ProductId} não está ativo.",
                productField));

        Promotion? promotion = null;
        if (promotionTask is not null)
        {
            promotion = ResolvePromotion(
                item,
                promotionField,
                await promotionTask,
                errors);
            if (promotion is { IsActive: false })
                errors.Add(new AppError(
                    $"A promoção ID {item.PromotionId!.Value} não está ativa.",
                    promotionField));
        }

        CalculatePromotion? calculatedPromotion = null;
        if (item.PromotionId is not null &&
            game is { IsActive: true } &&
            promotion is { IsActive: true })
        {
            var calculationResponse = await promotionsGateway
                .PostCalcularDescontoAsync(
                    item.PromotionId.Value,
                    item.ProductId,
                    game.BasePrice,
                    cancellationToken);
            calculatedPromotion = ResolvePromotionCalculation(
                item,
                promotionField,
                calculationResponse,
                errors);
        }

        if (errors.Count > 0)
            return new ItemValidationResult(null, errors);

        var finalPrice = item.PromotionId is null
            ? game!.BasePrice
            : calculatedPromotion!.FinalPrice;

        var purchaseOrderItem = PurchaseOrderItem.Create(
            item.ProductId,
            game!.BasePrice,
            finalPrice,
            item.PromotionId);

        return new ItemValidationResult(purchaseOrderItem, errors);
    }

    private static Game? ResolveGame(
        CreatePurchaseOrderItemsInput item,
        string field,
        ExternalApiResponse<Game> response,
        ICollection<AppError> errors)
    {
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            errors.Add(new AppError(
                $"Não foi localizado um jogo com o ID {item.ProductId}.",
                field));
            return null;
        }

        return GetSuccessfulResponse(response, "Catalog");
    }

    private static Promotion? ResolvePromotion(
        CreatePurchaseOrderItemsInput item,
        string field,
        ExternalApiResponse<Promotion> response,
        ICollection<AppError> errors)
    {
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            errors.Add(new AppError(
                $"Não foi localizada uma promoção com o ID {item.PromotionId!.Value}.",
                field));
            return null;
        }

        return GetSuccessfulResponse(response, "Promotions");
    }

    private static CalculatePromotion? ResolvePromotionCalculation(
        CreatePurchaseOrderItemsInput item,
        string field,
        ExternalApiResponse<CalculatePromotion> response,
        ICollection<AppError> errors)
    {
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            errors.Add(new AppError(
                $"Não foi localizada uma promoção com o ID {item.PromotionId!.Value}.",
                field));
            return null;
        }

        if (response.StatusCode == HttpStatusCode.UnprocessableEntity)
        {
            errors.Add(new AppError(
                string.IsNullOrWhiteSpace(response.ErrorDetail)
                    ? "A promoção informada não pode ser aplicada a este produto."
                    : response.ErrorDetail,
                field));
            return null;
        }

        return GetSuccessfulResponse(response, "Promotions");
    }

    private static T GetSuccessfulResponse<T>(
        ExternalApiResponse<T> response,
        string serviceName)
        where T : class
    {
        if (response.IsSuccessStatusCode && response.Value is not null)
            return response.Value;

        throw new HttpRequestException(
            $"A API {serviceName} respondeu com status {(int)response.StatusCode}.",
            inner: null,
            response.StatusCode);
    }

    private static IEnumerable<AppError> GetDuplicateItemErrors(
        IReadOnlyList<CreatePurchaseOrderItemsInput> items)
    {
        var indexedItems = items
            .Select((item, index) => new { Item = item, Index = index });

        return indexedItems
            .GroupBy(entry => entry.Item.ProductId)
            .Where(group => group.Count() > 1)
            .SelectMany(group => group.Select(entry => new AppError(
                $"O produto {group.Key} está repetido na ordem de compra.",
                $"items[{entry.Index}].productId")));
    }

    private sealed record ItemValidationResult(
        PurchaseOrderItem? Item,
        IReadOnlyCollection<AppError> Errors);
}
