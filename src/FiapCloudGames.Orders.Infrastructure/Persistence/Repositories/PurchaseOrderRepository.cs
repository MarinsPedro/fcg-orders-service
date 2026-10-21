namespace FiapCloudGames.Orders.Infrastructure.Persistence.Repositories;

internal sealed class PurchaseOrderRepository(OrdersDbContext dbContext) : IPurchaseOrderRepository
{
    public async Task AddAsync(PurchaseOrder purchaseOrder, CancellationToken cancellationToken)
    {
        await dbContext.PurchaseOrders.AddAsync(purchaseOrder, cancellationToken);
    }

    public async Task<PurchaseOrder?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await dbContext.PurchaseOrders.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        return result;
    }
}
