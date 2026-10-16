using FiapCloudGames.Orders.Application.Features.PurchaseOrders;

namespace FiapCloudGames.Orders.Presentation.Features.PurchaseOrders;

internal static class PurchaseOrderHttpMappings
{
    public static PurchaseOrderResponse ToResponse(this PurchaseOrderResult result) =>
        new(result.Id,
            result.CustomerId,
            result.CreatedAtUtc);
            
}
