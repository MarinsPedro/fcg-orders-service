using MassTransit;

namespace FiapCloudGames.Orders.Contracts.Events.V1;

[MessageUrn("GameGrantRequestedEventV1", useDefaultPrefix: true)]
public sealed record GameGrantRequestedEventV1(
    Guid EventId,
    Guid OrderId,
    Guid UserId,
    Guid GameId,
    DateTimeOffset OccurredAtUtc) : IIntegrationEvent;
