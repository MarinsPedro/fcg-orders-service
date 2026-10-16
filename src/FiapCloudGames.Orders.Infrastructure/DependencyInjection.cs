namespace FiapCloudGames.Orders.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddOrdersInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        #region Repositories

        var connectionString = configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException("A connection string 'Database' não foi configurada.");

        services.AddOrdersApplication();
        services.AddDbContext<OrdersDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IOrdersUnitOfWork>(provider => provider.GetRequiredService<OrdersDbContext>());
        services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
        services.AddHealthChecks().AddCheck<OrdersDatabaseHealthCheck>("orders-database", tags: ["ready"]);

        #endregion

        #region External Service API Rest

        services.AddFiapServiceTokenProvider(configuration);

        services.AddOptions<CatalogApiOptions>()
            .Bind(configuration.GetSection(CatalogApiOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddHttpClient<ICatalogGateway, CatalogClient>((provider, client) =>
            {
                var options = provider.GetRequiredService<IOptions<CatalogApiOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseAddress);
                client.Timeout = TimeSpan.FromSeconds(10);
            })
            .AddFiapCorrelationPropagation();

        #endregion

        return services;
    }
}
