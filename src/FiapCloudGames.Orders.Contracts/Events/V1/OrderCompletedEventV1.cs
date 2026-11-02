using MassTransit;

namespace FiapCloudGames.Orders.Contracts.Events.V1;

[MessageUrn("OrderCompletedEventV1", useDefaultPrefix: true)]
public sealed record OrderCompletedEventV1(
    Guid EventId,
    Guid OrderId,
    Guid UserId,
    DateTimeOffset CompletedAtUtc) : IIntegrationEvent;
