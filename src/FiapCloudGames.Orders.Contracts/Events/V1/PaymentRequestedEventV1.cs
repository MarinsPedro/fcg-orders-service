namespace FiapCloudGames.Orders.Contracts.Events.V1;

public sealed record PaymentRequestedEventV1(
    Guid EventId,
    Guid OrderId,
    Guid UserId,
    decimal Amount,
    string IdempotencyKey,
    DateTimeOffset OccurredAtUtc) : IIntegrationEvent;
