using FiapCloudGames.Orders.Domain.Orders;

namespace FiapCloudGames.Orders.Presentation.Features.PurchaseOrders;

public sealed record ChangeStatusPurchaseOrderRequest(
    [Required] Guid Id,
    [Required] PurchaseOrderStatus Status);

