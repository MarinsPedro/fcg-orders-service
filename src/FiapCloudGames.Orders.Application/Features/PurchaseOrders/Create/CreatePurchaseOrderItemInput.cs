namespace FiapCloudGames.Orders.Application.Features.PurchaseOrders.Create;

public sealed record CreatePurchaseOrderItemInput(
    Guid ProductId,
    decimal BasePrice,
    decimal FinalPrice,
    Guid? PromotionId);

