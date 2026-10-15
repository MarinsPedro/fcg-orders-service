namespace FiapCloudGames.Orders.Application.Features.PurchaseOrders;

internal static class OrdersApplicationMappings
{
    public static PurchaseOrderResult ToResult(PurchaseOrder purchaseOrder) =>
        new(purchaseOrder.Id,
            purchaseOrder.CustomerId,
            purchaseOrder.CreatedAtUtc);
}
