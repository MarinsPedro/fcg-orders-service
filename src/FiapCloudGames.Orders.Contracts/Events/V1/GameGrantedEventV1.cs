namespace FiapCloudGames.Orders.Contracts.Events.V1;

public sealed record GameGrantedEventV1(
    Guid EventId,
    Guid OrderId,
    Guid UserId,
    Guid GameId,
    DateTimeOffset GrantedAtUtc) : IIntegrationEvent;
