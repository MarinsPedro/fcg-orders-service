namespace FiapCloudGames.Orders.Infrastructure.Persistence.Repositories;

internal sealed class PurchaseOrderRepository(OrdersDbContext dbContext) : IPurchaseOrderRepository
{
    public async Task AddAsync(PurchaseOrder purchaseOrder, CancellationToken cancellationToken)
    {
        await dbContext.PurchaseOrders.AddAsync(purchaseOrder, cancellationToken);
    }

    public async Task AddReservationsAsync(
        IEnumerable<PurchaseOrderGameReservation> reservations,
        CancellationToken cancellationToken)
    {
        await dbContext.PurchaseOrderGameReservations.AddRangeAsync(
            reservations,
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<Guid>> GetGamesInActiveOrdersAsync(
        Guid customerId,
        IReadOnlyCollection<Guid> gameIds,
        CancellationToken cancellationToken)
    {
        var activeStatuses = new[]
        {
            PurchaseOrderStatus.Received,
            PurchaseOrderStatus.PendingPayment,
            PurchaseOrderStatus.PaymentApproved
        };

        return await (
                from order in dbContext.PurchaseOrders.AsNoTracking()
                join item in dbContext.PurchaseOrderItems.AsNoTracking()
                    on order.Id equals item.PurchaseOrderId
                where order.CustomerId == customerId &&
                      activeStatuses.Contains(order.Status) &&
                      gameIds.Contains(item.ProductId)
                select item.ProductId)
            .Distinct()
            .ToArrayAsync(cancellationToken);
    }

    public async Task<PurchaseOrder?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await dbContext.PurchaseOrders.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        return result;
    }
}
