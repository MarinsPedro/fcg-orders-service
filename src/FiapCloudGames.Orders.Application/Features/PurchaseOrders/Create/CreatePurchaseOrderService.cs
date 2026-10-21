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
        // 1. Consulta a api externa Library pra verificar se os jogos já constam na biblioteca do usuário.
        var libraryResults = await Task.WhenAll(
            input.items.Select(async item => new
            {
                Item = item,
                Library = await libraryGateway.CheckGameExistsOnLibraryAsync(
                    input.CustomerId,
                    item.ProductId,
                    cancellationToken)
            }));

        // 2. Mapeando os objetos de input para domínio e aplicando consultas as API Catalog e Promotions.
        var purchaseOrderItems = await Task.WhenAll(
            libraryResults
                .Where(result => !result.Library.Exists)
                .DistinctBy(x => x.Item.ProductId)
                .Select(async result =>
                {
                    var item = result.Item;
                    var game = await catalogGateway.GetGameByIdAsync(item.ProductId, cancellationToken);

                    CheckGame(result.Item, game);

                    CalculatePromotion calculatedPromotion = new(0);

                    if (item.PromotionId is not null)
                    {
                        var promotion = await promotionsGateway.GetByIdAsync(item.PromotionId.Value, cancellationToken);

                        CheckPromotions(result.Item, promotion);

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

        // 3. Verifica se existe pelo menos um produto na ordem de compra.
        if (!purchaseOrderItems.Any())
            throw AppException.Conflict("Não pode ser gerado ordem de compra sem produtos.");

        // 4. crio o objeto.
        var purchaseOrder = PurchaseOrder.Create(input.CustomerId, purchaseOrderItems, clock.GetUtcNow());

        // 5. Workaround para vincular o número da ordem da compra a cada item da compra.
        foreach (var item in purchaseOrder.Items)
        {
            logger.LogInformation("Vinculando o produto {ProductId} à ordem de compra {Id}", item.ProductId, purchaseOrder.Id);
            item.BindItemsToPurchaseOrder(purchaseOrder.Id);
        }

        logger.LogInformation("Gerando ordem de compra.");

        // 6. Gerando ordem de compra.
        await purchaseOrderRepository.AddAsync(purchaseOrder, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Ordem de compra criada em {CreatedAt} as {CreatedAtHour}.",
            purchaseOrder.CreatedAtUtc.ToString("dd/MM/yyyy"),
            purchaseOrder.CreatedAtUtc.ToString("HH:mm:ss.fff"));

        return OrdersApplicationMappings.ToResult(purchaseOrder);
    }

    private static void CheckGame(CreatePurchaseOrderItemsInput item, Game game)
    {
        if (game is null)
            throw AppException.Conflict($"Não foi localizado um jogo com o ID {item.ProductId}");

        if (!game.IsActive)
            throw AppException.Conflict($"O jogo ID {item.ProductId} não está ativo.");
    }

    private static void CheckPromotions(CreatePurchaseOrderItemsInput item, Promotion promotion)
    {
        if (promotion is null)
            throw AppException.Conflict($"Não foi localizado uma promoção com o ID {item.PromotionId!.Value}");

        if (!promotion.IsActive)
            throw AppException.Conflict($"A promoção ID {item.PromotionId!.Value} não está ativa.");
    }
}
