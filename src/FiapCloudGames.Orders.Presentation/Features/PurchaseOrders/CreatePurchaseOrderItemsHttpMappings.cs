namespace FiapCloudGames.Orders.Presentation.Features.PurchaseOrders;

internal static class CreatePurchaseOrderItemsHttpMappings
{
    public static CreatePurchaseOrderItemsInput ToInput(this CreatePurchaseOrderItemsRequest request) =>
        new(request.ProductId,
            request.FinalPrice,
            request.PromotionId);
}
