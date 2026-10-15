namespace FiapCloudGames.Orders.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddOrdersnfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException("A connection string 'Database' não foi configurada.");

        services.AddOrdersApplication();
        services.AddDbContext<OrdersDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IOrdersUnitOfWork>(provider => provider.GetRequiredService<OrdersDbContext>());
        services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
        services.AddHealthChecks().AddCheck<OrdersDatabaseHealthCheck>("orders-database", tags: ["ready"]);

        return services;
    }
}
