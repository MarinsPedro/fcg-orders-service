using System.Diagnostics;
using FiapCloudGames.BuildingBlocks.Observability;
using FiapCloudGames.Orders.Application.Common;

namespace FiapCloudGames.Orders.Presentation.Errors;

internal sealed class BusinessRuleValidationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (BusinessRuleValidationException exception)
            when (!context.Response.HasStarted)
        {
            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;

            var problemDetails = new ApiProblemDetails
            {
                Type = ApiProblemTypes.BusinessRule,
                Title = "Regra de negócio não atendida",
                Status = StatusCodes.Status422UnprocessableEntity,
                Detail = exception.Message,
                TraceId = Activity.Current?.TraceId.ToString() ??
                    context.TraceIdentifier,
                CorrelationId = context.Response.Headers[
                    CorrelationIdConstants.HeaderName].FirstOrDefault(),
                Errors = exception.Errors
                    .Select(error => new ApiError
                    {
                        Message = error.Message,
                        Field = error.Field
                    })
                    .ToArray()
            };

            await context.Response.WriteAsJsonAsync(
                problemDetails,
                options: null,
                contentType: FiapApiDefaultsExtensions.ProblemJsonContentType,
                cancellationToken: context.RequestAborted);
        }
    }
}
