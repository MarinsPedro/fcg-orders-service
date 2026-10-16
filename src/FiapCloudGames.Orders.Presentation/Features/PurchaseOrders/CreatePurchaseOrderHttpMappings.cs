namespace FiapCloudGames.Orders.Presentation.Features.PurchaseOrders;

internal static class CreatePurchaseOrderHttpMappings
{
    public static CreatePurchaseOrderInput ToInput(this CreatePurchaseOrderRequest request) =>
        new(request.CustomerId,
            request.items.Select(CreatePurchaseOrderItemsHttpMappings.ToInput));
}
