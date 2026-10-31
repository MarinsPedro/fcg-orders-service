using FiapCloudGames.Orders.Contracts.Events.V1;
using FiapCloudGames.Orders.Infrastructure.Messaging;
using MassTransit;

namespace FiapCloudGames.Orders.Worker.Messaging;

internal static class RabbitMqTopologyConfigurator
{
    // Exchange e routing keys compativeis com os consumers existentes em Payments e Library.
    private const string SharedEventsExchange = "fcg.events";

    internal static void Configure(
        IRabbitMqBusFactoryConfigurator rabbitMq,
        RabbitMqExchangeOptions exchanges)
    {
        // Permite publicacao sem envelope MassTransit (application/json).
        rabbitMq.AddRawJsonSerializer();

        rabbitMq.Message<PurchaseOrderCreatedEvent>(configuration =>
            configuration.SetEntityName(exchanges.PurchaseOrderCreated));

        rabbitMq.Message<PaymentRequestedEventV1>(configuration =>
            configuration.SetEntityName(SharedEventsExchange));
        rabbitMq.Send<PaymentRequestedEventV1>(configuration =>
        {
            configuration.UseRoutingKeyFormatter(_ => "payments.requested");
            configuration.UseSerializer("application/json");
        });
        rabbitMq.Publish<PaymentRequestedEventV1>(configuration =>
            configuration.ExchangeType = "topic");

        rabbitMq.Message<GameGrantRequestedEventV1>(configuration =>
            configuration.SetEntityName(SharedEventsExchange));
        rabbitMq.Send<GameGrantRequestedEventV1>(configuration =>
        {
            configuration.UseRoutingKeyFormatter(_ => "library.game.grant.requested");
            configuration.UseSerializer("application/json");
        });
        rabbitMq.Publish<GameGrantRequestedEventV1>(configuration =>
            configuration.ExchangeType = "topic");

        // Eventos internos destinados a PurchaseOrderStateMachine.
        rabbitMq.Message<PaymentApprovedEventV1>(configuration =>
            configuration.SetEntityName(exchanges.PaymentApproved));
        rabbitMq.Message<PaymentRejectedEventV1>(configuration =>
            configuration.SetEntityName(exchanges.PaymentRejected));
        rabbitMq.Message<GameGrantedEventV1>(configuration =>
            configuration.SetEntityName(exchanges.GameGranted));
        rabbitMq.Message<OrderCompletedEventV1>(configuration =>
            configuration.SetEntityName(exchanges.OrderCompleted));
    }
}
