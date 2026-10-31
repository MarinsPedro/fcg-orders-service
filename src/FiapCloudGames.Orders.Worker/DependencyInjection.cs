using FiapCloudGames.Orders.Infrastructure.Messaging;
using FiapCloudGames.Orders.Infrastructure.Orchestration;
using FiapCloudGames.Orders.Infrastructure.Persistence;
using FiapCloudGames.Orders.Worker.Messaging;
using FiapCloudGames.Orders.Worker.Sagas.PurchaseOrders;
using FiapCloudGames.Orders.Worker.Sagas.PurchaseOrders.Activities;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FiapCloudGames.Orders.Worker;

public static class DependencyInjection
{
    public static IServiceCollection AddOrdersWorker(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException(
                "A connection string 'Database' não foi configurada.");

        services.AddDbContext<OrdersDbContext>(options =>
            options.UseNpgsql(connectionString));
        services.AddSingleton(TimeProvider.System);

        AddRabbitMqOptions(services, configuration);
        AddPurchaseOrderSagaOptions(services, configuration);
        AddPurchaseOrderSaga(services);

        return services;
    }

    private static void AddPurchaseOrderSagaOptions(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<PurchaseOrderSagaOptions>()
            .Bind(configuration.GetSection(PurchaseOrderSagaOptions.SectionName))
            .Validate(
                options =>
                    options.MissingInstanceRedeliveryIntervals.Length > 0 &&
                    options.MissingInstanceRedeliveryIntervals.All(
                        interval => interval > TimeSpan.Zero),
                "PurchaseOrderSaga:MissingInstanceRedeliveryIntervals deve possuir intervalos positivos.")
            .ValidateOnStart();
    }

    private static void AddRabbitMqOptions(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<RabbitMqOptions>()
            .Bind(configuration.GetSection(RabbitMqOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.Host),
                "RabbitMq:Host é obrigatório.")
            .Validate(
                options => options.Port > 0,
                "RabbitMq:Port deve ser maior que zero.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.VirtualHost),
                "RabbitMq:VirtualHost é obrigatório.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.Username),
                "RabbitMq:Username é obrigatório.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.Password),
                "RabbitMq:Password é obrigatório.")
            .Validate(
                options => HasConfiguredTopology(options.Topology),
                "Todos os nomes em RabbitMq:Topology são obrigatórios.")
            .ValidateOnStart();
    }

    private static void AddPurchaseOrderSaga(IServiceCollection services)
    {
        services.AddScoped<InitializePurchaseOrderActivity>();
        services.AddScoped<ApprovePurchaseOrderPaymentActivity>();
        services.AddScoped<RejectPurchaseOrderPaymentActivity>();
        services.AddScoped<GrantPurchaseOrderGameActivity>();

        services.AddMassTransit(configurator =>
        {
            configurator.AddConsumer<PaymentOutcomeConsumer>();
            configurator.AddConsumer<LibraryGrantConsumer>();
            configurator.AddDelayedMessageScheduler();
            configurator.AddSagaStateMachine<
                    PurchaseOrderStateMachine,
                    PurchaseOrderSagaState,
                    PurchaseOrderStateMachineDefinition>()
                .EntityFrameworkRepository(repository =>
                {
                    repository.ConcurrencyMode = ConcurrencyMode.Pessimistic;
                    repository.ExistingDbContext<OrdersDbContext>();
                    repository.UsePostgres();
                });

            configurator.AddEntityFrameworkOutbox<OrdersDbContext>(outbox =>
            {
                outbox.UsePostgres();
            });

            configurator.UsingRabbitMq((context, rabbitMq) =>
            {
                var options = context
                    .GetRequiredService<IOptions<RabbitMqOptions>>()
                    .Value;

                RabbitMqTopologyConfigurator.Configure(
                    rabbitMq,
                    options.Topology.Exchanges);

                rabbitMq.UseDelayedMessageScheduler();

                rabbitMq.Host(
                    options.Host,
                    options.Port,
                    options.VirtualHost,
                    hostConfiguration =>
                    {
                        hostConfiguration.Username(options.Username);
                        hostConfiguration.Password(options.Password);
                    });

                // Filas duraveis ja provisionadas por rabbitmq/definitions.json.
                // Os endpoints usam JSON puro; a saga continua usando eventos internos MT.
                rabbitMq.ReceiveEndpoint("fcg.orders.payments", endpoint =>
                {
                    endpoint.ConfigureConsumeTopology = false;
                    endpoint.UseRawJsonDeserializer(RawSerializerOptions.All, isDefault: true);
                    endpoint.UseMessageRetry(retry => retry.Intervals(
                        TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(5)));
                    endpoint.UseEntityFrameworkOutbox<OrdersDbContext>(context);
                    endpoint.Bind("fcg.payments.events", binding =>
                    {
                        binding.ExchangeType = "topic";
                        binding.RoutingKey = "payments.paid";
                    });
                    endpoint.Bind("fcg.payments.events", binding =>
                    {
                        binding.ExchangeType = "topic";
                        binding.RoutingKey = "payments.failed";
                    });
                    endpoint.ConfigureConsumer<PaymentOutcomeConsumer>(context);
                });

                rabbitMq.ReceiveEndpoint("fcg.orders.library", endpoint =>
                {
                    endpoint.ConfigureConsumeTopology = false;
                    endpoint.UseRawJsonDeserializer(RawSerializerOptions.All, isDefault: true);
                    endpoint.UseMessageRetry(retry => retry.Intervals(
                        TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(5)));
                    endpoint.UseEntityFrameworkOutbox<OrdersDbContext>(context);
                    endpoint.Bind("fcg.library.events", binding =>
                    {
                        binding.ExchangeType = "topic";
                        binding.RoutingKey = "library.game.granted";
                    });
                    endpoint.ConfigureConsumer<LibraryGrantConsumer>(context);
                });

                // MassTransit configura a fila propria da saga usando os contratos internos.
                rabbitMq.ConfigureEndpoints(context);
            });
        });
    }

    private static bool HasConfiguredTopology(RabbitMqTopologyOptions topology)
    {
        var exchanges = topology.Exchanges;

        return !string.IsNullOrWhiteSpace(exchanges.PurchaseOrderCreated) &&
               !string.IsNullOrWhiteSpace(exchanges.PaymentRequested) &&
               !string.IsNullOrWhiteSpace(exchanges.PaymentApproved) &&
               !string.IsNullOrWhiteSpace(exchanges.PaymentRejected) &&
               !string.IsNullOrWhiteSpace(exchanges.GameGrantRequested) &&
               !string.IsNullOrWhiteSpace(exchanges.GameGranted) &&
               !string.IsNullOrWhiteSpace(exchanges.OrderCompleted) &&
               !string.IsNullOrWhiteSpace(topology.Queues.PurchaseOrderSaga);
    }
}
