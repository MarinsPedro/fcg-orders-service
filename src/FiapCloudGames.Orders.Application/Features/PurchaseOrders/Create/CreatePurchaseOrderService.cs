namespace FiapCloudGames.Orders.Application.Features.PurchaseOrders.Create;

public sealed class CreatePurchaseOrderService(
    IPurchaseOrderRepository purchaseOrderRepository,
    ICatalogGateway catalogGateway,
    IOrdersUnitOfWork unitOfWork,
    TimeProvider clock,
    ILogger<CreatePurchaseOrderService> logger)
{
    public async Task<PurchaseOrderResult> ExecuteAsync(
        CreatePurchaseOrderInput input,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Gerando ordem de compra.");

        var purchaseOrderItems = input.items
            .Select(purchaseOrderItems => PurchaseOrderItem
                .Create(
                    purchaseOrderItems.ProductId,
                    catalogGateway.GetGameByIdAsync(purchaseOrderItems.ProductId, cancellationToken).Result.BasePrice, // Workaround.
                    purchaseOrderItems.FinalPrice, // ms Promotions
                    purchaseOrderItems.PromotionId // ms Promotions
                )
            );

        var purchaseOrder = PurchaseOrder.Create(input.CustomerId, purchaseOrderItems, clock.GetUtcNow());

        // Workaround para vincular o número da ordem da compra a cada item da compra.
        foreach (var item in purchaseOrder.Items)
            item.BindItemsToPurchaseOrder(purchaseOrder.Id);

        await purchaseOrderRepository.AddAsync(purchaseOrder, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Ordem de compra criada em {CreatedAt} as {CreatedAtHour}.",
            purchaseOrder.CreatedAtUtc.ToString("dd/MM/yyyy"),
            purchaseOrder.CreatedAtUtc.ToString("HH:mm:ss.fff"));

        return OrdersApplicationMappings.ToResult(purchaseOrder);
    }
}
