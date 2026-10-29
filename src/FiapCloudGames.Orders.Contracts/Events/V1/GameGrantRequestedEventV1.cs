namespace FiapCloudGames.Orders.Contracts.Events.V1;

public sealed record GameGrantRequestedEventV1(
    Guid EventId,
    Guid OrderId,
    Guid UserId,
    Guid GameId,
    DateTimeOffset OccurredAtUtc) : IIntegrationEvent;
