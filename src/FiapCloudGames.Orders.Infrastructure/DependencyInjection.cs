using FiapCloudGames.Orders.Contracts.Events.V1;

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

        services.AddOptions<PromotionsApiOptions>()
            .Bind(configuration.GetSection(PromotionsApiOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<LibraryApiOptions>()
            .Bind(configuration.GetSection(LibraryApiOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddHttpClient<ICatalogGateway, CatalogClient>((provider, client) =>
        {
            var options = provider.GetRequiredService<IOptions<CatalogApiOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseAddress);
            client.Timeout = TimeSpan.FromSeconds(300); // default 10
        })
        .AddFiapCorrelationPropagation();

        services.AddHttpClient<IPromotionsGateway, PromotionsClient>((provider, client) =>
        {
            var options = provider.GetRequiredService<IOptions<PromotionsApiOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseAddress);
            client.Timeout = TimeSpan.FromSeconds(300);
        })
        .AddFiapCorrelationPropagation();

        services.AddHttpClient<ILibraryGateway, LibraryClient>((provider, client) => {
            var options = provider.GetRequiredService<IOptions<LibraryApiOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseAddress);
            client.Timeout = TimeSpan.FromSeconds(300);
        });

        #endregion

        #region Messaging

        var useInMemoryMessaging = configuration.GetValue<bool>("Messaging:UseInMemory");

        services
            .AddOptions<RabbitMqOptions>()
            .Bind(configuration.GetSection(RabbitMqOptions.SectionName))
            .Validate(
                options => useInMemoryMessaging ||
                    !string.IsNullOrWhiteSpace(options.Host),
                "RabbitMq:Host é obrigatório.")
            .Validate(
                options => useInMemoryMessaging || options.Port > 0,
                "RabbitMq:Port deve ser maior que zero.")
            .Validate(
                options => useInMemoryMessaging ||
                    !string.IsNullOrWhiteSpace(options.VirtualHost),
                "RabbitMq:VirtualHost é obrigatório.")
            .Validate(
                options => useInMemoryMessaging ||
                    !string.IsNullOrWhiteSpace(options.Username),
                "RabbitMq:Username é obrigatório.")
            .Validate(
                options => useInMemoryMessaging ||
                    !string.IsNullOrWhiteSpace(options.Password),
                "RabbitMq:Password é obrigatório.")
            .ValidateOnStart();
        services.AddMassTransit(configurator =>
        {
            if (useInMemoryMessaging)
            {
                configurator.UsingInMemory();
                return;
            }

            configurator.AddEntityFrameworkOutbox<OrdersDbContext>(outbox =>
            {
                outbox.UsePostgres();
                outbox.UseBusOutbox();
            });
            configurator.UsingRabbitMq((context, rabbitMq) =>
            {
                rabbitMq.Message<PurchaseOrderCreatedEvent>(topology =>
                    topology.SetEntityName(PurchaseOrderCreatedEvent.ExchangeName));

                var options = context.GetRequiredService<
                    Microsoft.Extensions.Options.IOptions<RabbitMqOptions>>().Value;
                rabbitMq.Host(
                    options.Host,
                    options.Port,
                    options.VirtualHost,
                    host =>
                    {
                        host.Username(options.Username);
                        host.Password(options.Password);
                    });
            });
        });

        services.AddScoped<IIntegrationEventPublisher, MassTransitIntegrationEventPublisher>();

        #endregion

        return services;
    }
}
