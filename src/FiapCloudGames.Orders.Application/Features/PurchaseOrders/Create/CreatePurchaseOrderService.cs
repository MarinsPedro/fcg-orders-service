namespace FiapCloudGames.Orders.Application.Features.PurchaseOrders.Create;

public sealed class CreatePurchaseOrderService(
    IPurchaseOrderRepository purchaseOrderRepository,
    ICatalogGateway catalogGateway,
    IPromotionsGateway promotionsGateway,
    IOrdersUnitOfWork unitOfWork,
    TimeProvider clock,
    ILogger<CreatePurchaseOrderService> logger)
{
    public async Task<PurchaseOrderResult> ExecuteAsync(
        CreatePurchaseOrderInput input,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Gerando ordem de compra.");

        //1. Mapeando os objetos de input para domínio.
        var purchaseOrderItems = await Task.WhenAll
            (
                input.items
                .Select(async purchaseOrderItems =>
                {
                    var game = await catalogGateway.GetGameByIdAsync(purchaseOrderItems.ProductId, cancellationToken);
                    CalculatePromotion calculatedPromotion = new(0);

                    // Somente faz o cálculo se um jogo possuir campanha de promoção.
                    if (purchaseOrderItems.PromotionId is not null)
                        calculatedPromotion = await promotionsGateway
                            .PostCalcularDescontoAsync(purchaseOrderItems.PromotionId.Value, purchaseOrderItems.ProductId, game.BasePrice, cancellationToken);

                    var finalPrice = purchaseOrderItems.PromotionId is null
                        ? game.BasePrice
                        : calculatedPromotion.FinalPrice;

                    return PurchaseOrderItem.Create(
                        purchaseOrderItems.ProductId,
                        game.BasePrice,
                        finalPrice,
                        purchaseOrderItems.PromotionId);
                })
            ) as IEnumerable<PurchaseOrderItem>;

        // 2. crio o objeto.
        var purchaseOrder = PurchaseOrder.Create(input.CustomerId, purchaseOrderItems, clock.GetUtcNow());

        // 3. Workaround para vincular o número da ordem da compra a cada item da compra.
        foreach (var item in purchaseOrder.Items)
            item.BindItemsToPurchaseOrder(purchaseOrder.Id);

        // 4. Gravar a ordem de compra.
        await purchaseOrderRepository.AddAsync(purchaseOrder, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Ordem de compra criada em {CreatedAt} as {CreatedAtHour}.",
            purchaseOrder.CreatedAtUtc.ToString("dd/MM/yyyy"),
            purchaseOrder.CreatedAtUtc.ToString("HH:mm:ss.fff"));

        return OrdersApplicationMappings.ToResult(purchaseOrder);
    }
}
