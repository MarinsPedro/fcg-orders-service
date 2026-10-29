using System.Text.Json;

namespace FiapCloudGames.Orders.Infrastructure.ExternalApi.Base;

internal static class ExternalApiResponseFactory
{
    public static ExternalApiResponse<T> Success<T>(
        HttpStatusCode statusCode,
        T value) =>
        new(statusCode, value);

    public static async Task<ExternalApiResponse<T>> FailureAsync<T>(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        string? detail = null;

        try
        {
            var problem = await response.Content
                .ReadFromJsonAsync<ExternalApiProblemDetails>(cancellationToken);
            detail = problem?.Detail;
        }
        catch (JsonException)
        {
            // Respostas de erro fora do contrato Problem Details são tratadas
            // pelo chamador apenas com base no status HTTP.
        }
        catch (NotSupportedException)
        {
            // O content-type não é JSON; não expõe o corpo da dependência.
        }

        return new ExternalApiResponse<T>(response.StatusCode, ErrorDetail: detail);
    }

    private sealed record ExternalApiProblemDetails(string? Detail);
}
