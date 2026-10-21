namespace FiapCloudGames.Orders.Application.Features.PurchaseOrders.Create;

public sealed record CreatePurchaseOrderItemsInput(
    Guid ProductId,
    Guid? PromotionId);

