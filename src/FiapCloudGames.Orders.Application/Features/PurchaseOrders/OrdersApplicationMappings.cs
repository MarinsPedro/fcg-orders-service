namespace FiapCloudGames.Orders.Application.Features.PurchaseOrders;

internal static class OrdersApplicationMappings
{
    // TODO: retornar os jogos da compra também? Avaliar.
    public static PurchaseOrderResult ToResult(PurchaseOrder purchaseOrder) =>
        new(purchaseOrder.Id,
            purchaseOrder.CustomerId,
            purchaseOrder.CreatedAtUtc);
}
