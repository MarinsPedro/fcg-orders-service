namespace FiapCloudGames.Orders.Application.Features.PurchaseOrders.Get;

public sealed class GetPurchaseOrderService(
    IPurchaseOrderRepository purchaseOrderRepository,
    ILogger<GetPurchaseOrderService> logger)
{
    public async Task<PurchaseOrderResult> ExecuteAsync(Guid id, CancellationToken cancellationToken)
    {
        logger.LogInformation("Localizando orderm de compra com id {Id}", id);

        var purchaseOrder = await purchaseOrderRepository.GetAsync(id, cancellationToken)
            ?? throw AppException.NotFound("Ordem de compra não encontrada");

        logger.LogInformation("Orderm de compra encontrado: {Id}", purchaseOrder.Id);

        return OrdersApplicationMappings.ToResult(purchaseOrder);
    }
}
