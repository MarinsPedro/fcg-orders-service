namespace FiapCloudGames.Orders.Presentation.Features.PurchaseOrders;

public sealed record PurchaseOrderResponse(
    Guid Id,
    Guid CustomerId,
    DateTimeOffset CreatedAtUtc);

