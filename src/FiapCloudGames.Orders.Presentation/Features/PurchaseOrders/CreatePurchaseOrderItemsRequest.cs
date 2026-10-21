namespace FiapCloudGames.Orders.Presentation.Features.PurchaseOrders;

public sealed record CreatePurchaseOrderItemsRequest(
    [Required] Guid ProductId,
    Guid? PromotionId);

