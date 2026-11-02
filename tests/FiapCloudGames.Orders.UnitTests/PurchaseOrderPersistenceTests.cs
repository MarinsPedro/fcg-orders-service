using FiapCloudGames.Orders.Domain.Orders;
using FiapCloudGames.Orders.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FiapCloudGames.Orders.UnitTests;

public sealed class PurchaseOrderPersistenceTests
{
    [Fact]
    public async Task PurchaseOrder_ShouldMaterializeItemsIntoMutableBackingCollection()
    {
        var options = new DbContextOptionsBuilder<OrdersDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new OrdersDbContext(options);
        var item = PurchaseOrderItem.Create(
            Guid.NewGuid(),
            100m,
            90m,
            null);
        var order = PurchaseOrder.Create(
            Guid.NewGuid(),
            [item],
            new DateTimeOffset(2026, 10, 29, 12, 0, 0, TimeSpan.Zero));
        item.BindItemsToPurchaseOrder(order.Id);

        dbContext.PurchaseOrders.Add(order);
        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();

        var loadedOrder = await dbContext.PurchaseOrders
            .Include(purchaseOrder => purchaseOrder.Items)
            .SingleAsync(purchaseOrder => purchaseOrder.Id == order.Id);

        var loadedItem = Assert.Single(loadedOrder.Items);
        Assert.Equal(item.Id, loadedItem.Id);
    }
}
