namespace FiapCloudGames.Orders.Application.Abstractions.External;

public interface ICatalogGateway //TODO: talvez um nome seja ICatalogClient
{
    Task<Game> GetGameByIdAsync(Guid id, CancellationToken cancellationToken = default);
}

public sealed record Game(
    Guid Id,
    string Title,
    string Description,
    string Category,
    decimal BasePrice,
    bool IsActive);
