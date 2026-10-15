namespace FiapCloudGames.Orders.Application.Abstractions.Persistence;

public interface IOrdersUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
