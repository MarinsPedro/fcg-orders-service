namespace FiapCloudGames.Orders.Migrations.DesignTime;

public sealed class OrdersDbContextFactory : IDesignTimeDbContextFactory<OrdersDbContext>
{
    public OrdersDbContext CreateDbContext(string[] args)
    {
        var builder = MigrationConnection.CreateBuilder(args);
        var options = new DbContextOptionsBuilder<OrdersDbContext>();
        MigrationDbContextOptions.Configure(
            options,
            MigrationConnection.GetRequired(builder.Configuration));

        return new OrdersDbContext(options.Options);
    }
}
