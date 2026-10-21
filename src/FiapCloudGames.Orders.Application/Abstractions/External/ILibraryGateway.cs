namespace FiapCloudGames.Orders.Application.Abstractions.External;

public interface ILibraryGateway
{
    Task<UserLibrary> CheckGameExistsOnLibraryAsync(Guid userId, Guid gameId, CancellationToken cancellationToken = default);
}

public sealed record UserLibrary(bool Exists);
