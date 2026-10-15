namespace FiapCloudGames.Orders.Application.Features.PurchaseOrders.Create;

public sealed record PurchaseOrderItemInput(
    Guid ProductId,
    decimal BasePrice,
    decimal FinalPrice,
    Guid? PromotionId);

