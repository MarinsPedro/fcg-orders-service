namespace FiapCloudGames.Orders.Presentation.Features.PurchaseOrders;

public sealed record CreatePurchaseOrderItemsRequest(
    [Required] Guid ProductId,
    [Required] Guid PromotionId);

