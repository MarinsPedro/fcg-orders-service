namespace FiapCloudGames.Orders.Application.Abstractions.External;

public sealed record ExternalApiResponse<T>(
    HttpStatusCode StatusCode,
    T? Value = default,
    string? ErrorDetail = null)
{
    public bool IsSuccessStatusCode =>
        (int)StatusCode is >= 200 and <= 299;
}
