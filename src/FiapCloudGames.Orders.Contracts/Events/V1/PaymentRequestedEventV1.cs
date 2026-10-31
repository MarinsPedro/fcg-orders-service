namespace FiapCloudGames.Orders.Contracts.Events.V1;

// Publicado em fcg.events / payments.requested (JSON puro).
// IdempotencyKey faz parte do contrato de entrada de Payments.
public sealed record PaymentRequestedEventV1(
    Guid EventId,
    Guid OrderId,
    Guid UserId,
    decimal Amount,
    string IdempotencyKey,
    DateTimeOffset OccurredAtUtc) : IIntegrationEvent;
