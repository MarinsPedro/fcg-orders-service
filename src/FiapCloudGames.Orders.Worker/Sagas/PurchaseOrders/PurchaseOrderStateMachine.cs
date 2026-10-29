using FiapCloudGames.Orders.Contracts.Events.V1;
using FiapCloudGames.Orders.Infrastructure.Orchestration;
using FiapCloudGames.Orders.Worker.Sagas.PurchaseOrders.Activities;
using MassTransit;
using Microsoft.Extensions.Options;

namespace FiapCloudGames.Orders.Worker.Sagas.PurchaseOrders;

public sealed class PurchaseOrderStateMachine :
    MassTransitStateMachine<PurchaseOrderSagaState>
{
    public PurchaseOrderStateMachine(
        IOptions<PurchaseOrderSagaOptions> options,
        ILogger<PurchaseOrderStateMachine> logger)
    {
        var missingInstanceRedeliveryIntervals =
            options.Value.MissingInstanceRedeliveryIntervals;

        InstanceState(saga => saga.CurrentState);

        Event(() => PurchaseOrderCreated, configuration =>
        {
            configuration.CorrelateById(context =>
                context.Message.PurchaseOrderId);
            configuration.SelectId(context =>
                context.Message.PurchaseOrderId);
        });

        Event(() => PaymentApproved, configuration =>
        {
            configuration.CorrelateById(context => context.Message.OrderId);
            configuration.OnMissingInstance(missing => missing.Redeliver(redelivery =>
            {
                redelivery.UseMessageScheduler = true;
                redelivery.Intervals(missingInstanceRedeliveryIntervals);
                redelivery.OnRedeliveryLimitReached(missingInstance =>
                    missingInstance.ExecuteAsync(context =>
                        FaultMissingSagaAsync(
                            context,
                            context.Message.OrderId,
                            context.Message.EventId,
                            logger)));
            }));
        });

        Event(() => PaymentRejected, configuration =>
        {
            configuration.CorrelateById(context => context.Message.OrderId);
            configuration.OnMissingInstance(missing => missing.Redeliver(redelivery =>
            {
                redelivery.UseMessageScheduler = true;
                redelivery.Intervals(missingInstanceRedeliveryIntervals);
                redelivery.OnRedeliveryLimitReached(missingInstance =>
                    missingInstance.ExecuteAsync(context =>
                        FaultMissingSagaAsync(
                            context,
                            context.Message.OrderId,
                            context.Message.EventId,
                            logger)));
            }));
        });

        Event(() => GameGranted, configuration =>
        {
            configuration.CorrelateById(context => context.Message.OrderId);
            configuration.OnMissingInstance(missing => missing.Redeliver(redelivery =>
            {
                redelivery.UseMessageScheduler = true;
                redelivery.Intervals(missingInstanceRedeliveryIntervals);
                redelivery.OnRedeliveryLimitReached(missingInstance =>
                    missingInstance.ExecuteAsync(context =>
                        FaultMissingSagaAsync(
                            context,
                            context.Message.OrderId,
                            context.Message.EventId,
                            logger)));
            }));
        });

        Initially(
            When(PurchaseOrderCreated)
                .Activity(activity =>
                    activity.OfType<InitializePurchaseOrderActivity>())
                .TransitionTo(WaitingForPayment));

        DuringAny(Ignore(PurchaseOrderCreated));

        During(WaitingForPayment,
            When(PaymentApproved)
                .Activity(activity =>
                    activity.OfType<ApprovePurchaseOrderPaymentActivity>())
                .TransitionTo(GrantingGames),
            When(PaymentRejected)
                .Activity(activity =>
                    activity.OfType<RejectPurchaseOrderPaymentActivity>())
                .TransitionTo(Rejected));

        During(GrantingGames,
            Ignore(PaymentApproved),
            Ignore(PaymentRejected),
            When(GameGranted)
                .Activity(activity =>
                    activity.OfType<GrantPurchaseOrderGameActivity>())
                .If(
                    context => context.Saga.AllGamesGranted,
                    completed => completed.TransitionTo(Completed)));

        During(Completed,
            Ignore(PaymentApproved),
            Ignore(PaymentRejected),
            Ignore(GameGranted));

        During(Rejected,
            Ignore(PaymentApproved),
            Ignore(PaymentRejected),
            Ignore(GameGranted));
    }

    public State WaitingForPayment { get; private set; } = null!;
    public State GrantingGames { get; private set; } = null!;
    public State Completed { get; private set; } = null!;
    public State Rejected { get; private set; } = null!;

    public Event<PurchaseOrderCreatedEvent> PurchaseOrderCreated { get; private set; } = null!;
    public Event<PaymentApprovedEventV1> PaymentApproved { get; private set; } = null!;
    public Event<PaymentRejectedEventV1> PaymentRejected { get; private set; } = null!;
    public Event<GameGrantedEventV1> GameGranted { get; private set; } = null!;

    private static Task FaultMissingSagaAsync<TMessage>(
        ConsumeContext<TMessage> context,
        Guid orderId,
        Guid eventId,
        ILogger logger)
        where TMessage : class
    {
        var messageType = typeof(TMessage).Name;

        logger.LogError(
            "A saga da ordem {OrderId} não foi localizada após as redeliveries. " +
            "A mensagem {MessageType}, evento {EventId}, será enviada para a error queue.",
            orderId,
            messageType,
            eventId);

        return Task.FromException(
            new MissingPurchaseOrderSagaException(
                $"A saga da ordem {orderId} não foi localizada para o evento {eventId} ({messageType})."));
    }
}
