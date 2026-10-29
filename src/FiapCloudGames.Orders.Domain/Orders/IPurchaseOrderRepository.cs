namespace FiapCloudGames.Orders.Domain.Orders;

public interface IPurchaseOrderRepository
{
    Task AddAsync(PurchaseOrder purchaseOrder, CancellationToken cancellationToken);
    Task AddReservationsAsync(
        IEnumerable<PurchaseOrderGameReservation> reservations,
        CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Guid>> GetGamesInActiveOrdersAsync(
        Guid customerId,
        IReadOnlyCollection<Guid> gameIds,
        CancellationToken cancellationToken);
    Task<PurchaseOrder?> GetAsync(Guid id, CancellationToken cancellationToken);
}
