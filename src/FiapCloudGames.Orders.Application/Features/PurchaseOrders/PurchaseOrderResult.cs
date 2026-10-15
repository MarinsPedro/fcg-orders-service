namespace FiapCloudGames.Orders.Application.Features.PurchaseOrders;

public sealed record PurchaseOrderResult(
    Guid Id,
    Guid CustomerId,
    DateTimeOffset createdAtUtc);

