namespace FiapCloudGames.Orders.Presentation.Features.PurchaseOrders;

internal static class ChangeStatusPurchaseOrderHttpMappings
{
    public static ChangeStatusPurchaseOrderInput ToInput(this ChangeStatusPurchaseOrderRequest request) =>
        new(request.Status);
}
