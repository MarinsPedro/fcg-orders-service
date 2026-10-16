namespace FiapCloudGames.Orders.Presentation.Features.PurchaseOrders;

public sealed record CreatePurchaseOrderRequest(
    [Required] Guid CustomerId,
    [Required] IEnumerable<CreatePurchaseOrderItemsRequest> items);

