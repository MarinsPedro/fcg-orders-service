namespace FiapCloudGames.Orders.Contracts.Events.V1;

public sealed record OrderCompletedEventV1(
    Guid EventId,
    Guid OrderId,
    Guid UserId,
    DateTimeOffset CompletedAtUtc) : IIntegrationEvent;
