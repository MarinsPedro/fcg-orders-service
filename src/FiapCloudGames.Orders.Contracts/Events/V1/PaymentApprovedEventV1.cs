namespace FiapCloudGames.Orders.Contracts.Events.V1;

public sealed record PaymentApprovedEventV1(
    Guid EventId,
    Guid PaymentId,
    Guid OrderId,
    Guid UserId,
    decimal Amount,
    string ProviderTransactionId,
    DateTimeOffset OccurredAtUtc) : IIntegrationEvent;
