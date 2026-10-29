namespace FiapCloudGames.Orders.Presentation.Features.PurchaseOrders;

[ApiController]
[Route("v1/purchase-order")]
public class PurchaseOrderController : ControllerBase
{
    private const string GetByIdRoute = "Orders.PurchaseOrder.GetByIdAsync";

    [Authorize(Policy = OrdersPolicies.PurchaseOrderRead)]
    [HttpGet("{id:guid}", Name = GetByIdRoute)]
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
    [ProducesResponseType(typeof(PurchaseOrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PurchaseOrderResponse>> CreateAsync(
        [FromBody] CreatePurchaseOrderRequest request,
        [FromServices] CreatePurchaseOrderService service,
        CancellationToken cancellationToken)
    {
        var result = await service.ExecuteAsync(request.ToInput(), cancellationToken);
        var response = result.ToResponse();
        return CreatedAtRoute(GetByIdRoute, new { id = response.Id }, response);
    }
}
