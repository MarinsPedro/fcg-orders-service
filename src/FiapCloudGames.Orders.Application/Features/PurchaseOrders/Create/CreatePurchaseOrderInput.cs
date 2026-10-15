namespace FiapCloudGames.Orders.Application.Features.PurchaseOrders.Create;

public sealed record CreatePurchaseOrderInput(
    Guid CustomerId,
    IEnumerable<CreatePurchaseOrderItemInput> items);

