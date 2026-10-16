namespace FiapCloudGames.Orders.Application.Features.PurchaseOrders.ChangeStatus;

// TODO: ver se o PurchaseOrderStatus também não deveria ser mapeado a parte e não simplesmente usar o objeto do Domain.
public sealed record ChangeStatusPurchaseOrderInput(PurchaseOrderStatus Status);

