namespace FiapCloudGames.Orders.Application.Features.PurchaseOrders.Create;

public sealed class CreatePurchaseOrderService(
    IPurchaseOrderRepository purchaseOrderRepository,
    ICatalogGateway catalogGateway,
    IPromotionsGateway promotionsGateway,
    ILibraryGateway libraryGateway,
    IOrdersUnitOfWork unitOfWork,
    TimeProvider clock,
    ILogger<CreatePurchaseOrderService> logger)
{
    public async Task<PurchaseOrderResult> ExecuteAsync(
        CreatePurchaseOrderInput input,
        CancellationToken cancellationToken)
    {
        // OBS: Se alguma das validações falhar não processa a compra, processo atômico, ou faz tudo ou faz nada.

        // 1. Verifica produtos repetidos nos items da ordem de compra.
        CheckDuplicateItem(input.items);

        // 2. Consulta a api externa Library pra verificar se os jogos já constam na biblioteca do usuário, caso ele já possua interrompe o processo de compra.
        await Task.WhenAll
        (
            input.items
                .Select(async item =>
                {
                    var isOwnedGame = await libraryGateway.CheckGameExistsOnLibraryAsync(
                        input.CustomerId,
                        item.ProductId,
                        cancellationToken);

                    if (isOwnedGame.Exists)
                        throw AppException.BusinessRule($"O cliente {input.CustomerId} já possui o produto {item.ProductId}");

                    return item;
                })
        );

        // 3. Mapeando os objetos de input para domínio e aplicando consultas as API Catalog e Promotions.
        var purchaseOrderItems = await Task.WhenAll(
            input.items
                .Select(async item =>
                {
                    var game = await catalogGateway.GetGameByIdAsync(item.ProductId, cancellationToken);

                    CheckGame(item, game);

                    CalculatePromotion calculatedPromotion = new(0);

                    if (item.PromotionId is not null)
                    {
                        var promotion = await promotionsGateway.GetByIdAsync(item.PromotionId.Value, cancellationToken);

                        CheckPromotions(item, promotion);

                        calculatedPromotion = await promotionsGateway
                            .PostCalcularDescontoAsync(
                                item.PromotionId.Value,
                                item.ProductId,
                                game.BasePrice,
                                cancellationToken);
                    }

                    var finalPrice = item.PromotionId is null
                        ? game.BasePrice
                        : calculatedPromotion.FinalPrice;

                    return PurchaseOrderItem.Create(
                        item.ProductId,
                        game.BasePrice,
                        finalPrice,
                        item.PromotionId);
                }));

        // 4. Verifica se existe pelo menos um produto na ordem de compra.
        if (!purchaseOrderItems.Any())
            throw AppException.Conflict("Não pode ser gerado ordem de compra sem produtos.");

        // 5. crio o objeto.
        var purchaseOrder = PurchaseOrder.Create(input.CustomerId, purchaseOrderItems, clock.GetUtcNow());

        // 6. Workaround para vincular o número da ordem da compra a cada item da compra.
        foreach (var item in purchaseOrder.Items)
        {
            logger.LogInformation("Vinculando o produto {ProductId} à ordem de compra {Id}", item.ProductId, purchaseOrder.Id);
            item.BindItemsToPurchaseOrder(purchaseOrder.Id);
        }

        logger.LogInformation("Gerando ordem de compra.");

        // 7. Gerando ordem de compra.
        await purchaseOrderRepository.AddAsync(purchaseOrder, cancellationToken);

        // OBS: Order é o orquestrador de tudo.

        // TODO: 0: (instalar uma imagem do RabbitMQ no docker)
        // TODO: 1 - await chamando o messtransit pra salvar (messtransit.Publish) o evento indicando que uma ordem de compra foi Recebida.

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // TODO: 2 - order worker (consumer 1 para enviar p/ pagto) fica ouvindo a mensagem publicada no RabbitMQ , quando consumir, dispara uma mensagem para criar um pagamento.
        // TODO: 3 - order worker (consumer 2 para receber do pagto se sucesso ou não).

        logger.LogInformation("Ordem de compra criada em {CreatedAt} as {CreatedAtHour}.",
            purchaseOrder.CreatedAtUtc.ToString("dd/MM/yyyy"),
            purchaseOrder.CreatedAtUtc.ToString("HH:mm:ss.fff"));

        return OrdersApplicationMappings.ToResult(purchaseOrder);

        // OBS: toda vez que algum ms bate no Order é um consumer.

        // TODO: depois de tudo, um outro consumer 3 do worker deverá enviar uma mensagem para o Library pra vincular o usário x List<Games>;
    }

    private static void CheckGame(CreatePurchaseOrderItemsInput item, Game game)
    {
        // OBS: Tudo que não é do domínio do ms é um BusinessRule;

        if (game is null)
            throw AppException.BusinessRule($"Não foi localizado um jogo com o ID {item.ProductId}");

        if (!game.IsActive)
            throw AppException.BusinessRule($"O jogo ID {item.ProductId} não está ativo.");
    }

    private static void CheckPromotions(CreatePurchaseOrderItemsInput item, Promotion promotion)
    {
        if (promotion is null)
            throw AppException.BusinessRule($"Não foi localizado uma promoção com o ID {item.PromotionId!.Value}");

        if (!promotion.IsActive)
            throw AppException.BusinessRule($"A promoção ID {item.PromotionId!.Value} não está ativa.");
    }

    private static void CheckDuplicateItem(IEnumerable<CreatePurchaseOrderItemsInput> items)
    {
        var duplicatedItems = items
            .GroupBy(item => item.ProductId)
            .Where(group => group.Count() > 1)
            .Select(group => new
            {
                ProductId = group.Key,
                AmountDuplicated = group.Count()
            })
            .ToList();

        if (duplicatedItems.Any())
        {
            var detalhesErro = string.Join(", ", duplicatedItems.Select(d => $"(ID: {d.ProductId}) apareceu {d.AmountDuplicated} vezes"));
            throw AppException.BusinessRule($"Operação cancelada! Foram encontrados produtos duplicados na lista: {detalhesErro}. Por favor, revise os itens.");
        }
    }
}
