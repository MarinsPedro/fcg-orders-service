namespace FiapCloudGames.Orders.Infrastructure.Persistence;

public sealed class OrdersDbContext(DbContextOptions<OrdersDbContext> options) : DbContext(options)
{
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderItem> PurchaseOrderItems => Set<PurchaseOrderItem>();
    public DbSet<PurchaseOrderGameReservation> PurchaseOrderGameReservations =>
        Set<PurchaseOrderGameReservation>();
    public DbSet<Orchestration.PurchaseOrderSagaState> PurchaseOrderSagaStates =>
        Set<Orchestration.PurchaseOrderSagaState>();
    public DbSet<Orchestration.PurchaseOrderSagaGame> PurchaseOrderSagaGames =>
        Set<Orchestration.PurchaseOrderSagaGame>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("orders");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrdersDbContext).Assembly);

        modelBuilder.AddInboxStateEntity(entity => entity.ToTable("inbox_state"));
        modelBuilder.AddOutboxMessageEntity(entity => entity.ToTable("outbox_message"));
        modelBuilder.AddOutboxStateEntity(entity => entity.ToTable("outbox_state"));
    }
}
