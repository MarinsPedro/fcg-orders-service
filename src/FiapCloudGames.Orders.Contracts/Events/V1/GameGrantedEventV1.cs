using MassTransit;

namespace FiapCloudGames.Orders.Contracts.Events.V1;

[MessageUrn("GameGrantedEventV1", useDefaultPrefix: true)]
public sealed record GameGrantedEventV1(
    Guid EventId,
    Guid OrderId,
    Guid UserId,
    Guid GameId,
    DateTimeOffset GrantedAtUtc) : IIntegrationEvent;
