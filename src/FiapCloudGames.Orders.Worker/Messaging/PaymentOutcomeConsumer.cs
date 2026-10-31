using FiapCloudGames.Orders.Contracts.Events.V1;
using MassTransit;

namespace FiapCloudGames.Orders.Worker.Messaging;

// Adapta eventos JSON de Payments aos eventos internos usados pela saga de Orders.
public sealed class PaymentOutcomeConsumer(
    IPublishEndpoint publishEndpoint,
    ILogger<PaymentOutcomeConsumer> logger) : IConsumer<PaymentOutcomeMessage>
{
    public async Task Consume(ConsumeContext<PaymentOutcomeMessage> context)
    {
        var payment = context.Message;
        if (payment.PaymentId == Guid.Empty ||
            payment.OrderId == Guid.Empty ||
            payment.UserId == Guid.Empty)
            throw new InvalidOperationException("Evento de pagamento com identificadores invalidos.");

        var isPaid = !string.IsNullOrWhiteSpace(payment.ProviderTransactionId);
        var isFailed = !string.IsNullOrWhiteSpace(payment.FailureReason);
        if (isPaid == isFailed)
            throw new InvalidOperationException("Evento de pagamento ambiguo: esperado exatamente um entre ProviderTransactionId e FailureReason.");

        var eventId = Guid.NewGuid();
        if (isPaid)
        {
            await publishEndpoint.Publish(
                new PaymentApprovedEventV1(
                    eventId, payment.OrderId, payment.UserId,
                    payment.Amount, payment.OccurredAtUtc),
                send =>
                {
                    send.MessageId = eventId;
                    send.CorrelationId = payment.OrderId;
                },
                context.CancellationToken);
            logger.LogInformation(
                "Payments paid -> Orders PaymentApproved. OrderId={OrderId}, PaymentId={PaymentId}",
                payment.OrderId, payment.PaymentId);
        }
        else
        {
            await publishEndpoint.Publish(
                new PaymentRejectedEventV1(
                    eventId, payment.OrderId, payment.UserId,
                    payment.Amount, payment.FailureReason!, payment.OccurredAtUtc),
                send =>
                {
                    send.MessageId = eventId;
                    send.CorrelationId = payment.OrderId;
                },
                context.CancellationToken);
            logger.LogWarning(
                "Payments failed -> Orders PaymentRejected. OrderId={OrderId}, PaymentId={PaymentId}",
                payment.OrderId, payment.PaymentId);
        }
    }
}
