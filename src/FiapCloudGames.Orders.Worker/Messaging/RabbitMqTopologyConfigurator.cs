using FiapCloudGames.Orders.Contracts.Events.V1;
using FiapCloudGames.Orders.Infrastructure.Messaging;
using MassTransit;

namespace FiapCloudGames.Orders.Worker.Messaging;

internal static class RabbitMqTopologyConfigurator
{
    internal static void Configure(
        IRabbitMqBusFactoryConfigurator rabbitMq,
        RabbitMqExchangeOptions exchanges)
    {
        rabbitMq.Message<PurchaseOrderCreatedEvent>(configuration =>
            configuration.SetEntityName(exchanges.PurchaseOrderCreated));
        rabbitMq.Message<PaymentRequestedEventV1>(configuration =>
            configuration.SetEntityName(exchanges.PaymentRequested));
        rabbitMq.Message<PaymentApprovedEventV1>(configuration =>
            configuration.SetEntityName(exchanges.PaymentApproved));
        rabbitMq.Message<PaymentRejectedEventV1>(configuration =>
            configuration.SetEntityName(exchanges.PaymentRejected));
        rabbitMq.Message<GameGrantRequestedEventV1>(configuration =>
            configuration.SetEntityName(exchanges.GameGrantRequested));
        rabbitMq.Message<GameGrantedEventV1>(configuration =>
            configuration.SetEntityName(exchanges.GameGranted));
        rabbitMq.Message<OrderCompletedEventV1>(configuration =>
            configuration.SetEntityName(exchanges.OrderCompleted));
    }
}
