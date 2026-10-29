using FiapCloudGames.Orders.Domain.Common;
using FiapCloudGames.Orders.Infrastructure.Messaging;
using FiapCloudGames.Orders.Infrastructure.Orchestration;
using FiapCloudGames.Orders.Infrastructure.Persistence;
using MassTransit;
using Microsoft.Extensions.Options;

namespace FiapCloudGames.Orders.Worker.Sagas.PurchaseOrders;

public sealed class PurchaseOrderStateMachineDefinition :
    SagaDefinition<PurchaseOrderSagaState>
{
    public PurchaseOrderStateMachineDefinition(IOptions<RabbitMqOptions> options)
    {
        EndpointName = options.Value.Topology.Queues.PurchaseOrderSaga;
        ConcurrentMessageLimit = 16;
    }

    protected override void ConfigureSaga(
        IReceiveEndpointConfigurator endpointConfigurator,
        ISagaConfigurator<PurchaseOrderSagaState> sagaConfigurator,
        IRegistrationContext context)
    {
        endpointConfigurator.PrefetchCount = 32;
        endpointConfigurator.UseMessageRetry(retry =>
        {
            retry.Ignore<DomainRuleViolationException>();
            retry.Ignore<MissingPurchaseOrderSagaException>();
            retry.Intervals(
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(3),
                TimeSpan.FromSeconds(5));
        });
        endpointConfigurator.UseEntityFrameworkOutbox<OrdersDbContext>(context);
    }
}
