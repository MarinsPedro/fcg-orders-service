namespace FiapCloudGames.Orders.Contracts.Events.V1;

public sealed record PaymentApprovedEventV1(
    Guid EventId,
    Guid OrderId,
    Guid UserId,
    decimal Amount,
    DateTimeOffset OccurredAtUtc) : IIntegrationEvent;
