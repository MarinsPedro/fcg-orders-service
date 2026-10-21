namespace FiapCloudGames.Orders.Presentation.Features.PurchaseOrders;

public sealed record ChangeStatusPurchaseOrderRequest([Required] PurchaseOrderStatus Status);

