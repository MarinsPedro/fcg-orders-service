namespace FiapCloudGames.Orders.Application.Features.PurchaseOrders.Create;

public sealed record CreatePurchaseOrderItemsInput(
    Guid ProductId,
    decimal FinalPrice,
    Guid? PromotionId);

