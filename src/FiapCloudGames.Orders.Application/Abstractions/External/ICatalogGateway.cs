namespace FiapCloudGames.Orders.Application.Abstractions.External;

public interface ICatalogGateway
{
    Task<ExternalApiResponse<Game>> GetGameByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}

public sealed record Game(
    Guid Id,
    string Title,
    string Description,
    string Category,
    decimal BasePrice,
    bool IsActive);
