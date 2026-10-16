using System.ComponentModel.DataAnnotations;

namespace FiapCloudGames.Orders.Presentation.Features.PurchaseOrders;

public sealed record CreatePurchaseOrderItemsRequest(
    [Required] Guid ProductId,
    [Required][Range(0, 9999999999.99)] decimal FinalPrice,
    [Required] Guid PromotionId);

