namespace FiapCloudGames.Orders.Presentation.Features.PurchaseOrders;

[ApiController]
[Route("v1/purchase-order")]
public class PurchaseOrderController : ControllerBase
{
    private const string GetByIdRoute = "Orders.PurchaseOrder.GetByIdAsync";

    [Authorize(Policy = OrdersPolicies.PurchaseOrderRead)]
    [HttpGet(Name = GetByIdRoute)]
    public async Task<ActionResult<PurchaseOrderResponse>> GetByIdAsync(
        [FromRoute] Guid id,
        [FromServices] GetPurchaseOrderService service,
        CancellationToken cancellationToken)
    {
        var result = await service.ExecuteAsync(id, cancellationToken);
        return Ok(result.ToResponse());
    }

    [Authorize(Policy = OrdersPolicies.PurchaseOrderCreate)]
    [HttpPost]
    public async Task<ActionResult<PurchaseOrderResponse>> CreateAsync(
        [FromBody] CreatePurchaseOrderRequest request,
        [FromServices] CreatePurchaseOrderService service,
        CancellationToken cancellationToken)
    {
        var result = await service.ExecuteAsync(request.ToInput(), cancellationToken);
        var response = result.ToResponse();
        return CreatedAtRoute(GetByIdRoute, new { id = response.Id }, response);
    }

    [Authorize(Policy = OrdersPolicies.PurchaseOrderWrite)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PurchaseOrderResponse>> PutChangeStatusAsync(
        [FromRoute] Guid id,
        [FromBody] ChangeStatusPurchaseOrderRequest request,
        [FromServices] ChangeStatusPurchaseOrderService service,
        CancellationToken cancellationToken)
    {
        var result = await service.ExecuteAsync(id, request.ToInput(), cancellationToken);
        var response = result.ToResponse();
        return CreatedAtRoute(GetByIdRoute, new { id = response.Id }, response);
    }
}
