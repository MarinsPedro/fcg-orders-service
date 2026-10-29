namespace FiapCloudGames.Orders.Contracts.Events.V1;

public sealed record PaymentRejectedEventV1(
    Guid EventId,
    Guid OrderId,
    Guid UserId,
    decimal Amount,
    string RejectionReason,
    DateTimeOffset OccurredAtUtc) : IIntegrationEvent;
