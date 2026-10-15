namespace FiapCloudGames.Orders.Domain.Orders;

public interface IPurchaseOrderRepository
{
    Task AddAsync(PurchaseOrder purchaseOrder, CancellationToken cancellationToken);
    Task<PurchaseOrder?> GetAsync(Guid id, CancellationToken cancellationToken);
}
