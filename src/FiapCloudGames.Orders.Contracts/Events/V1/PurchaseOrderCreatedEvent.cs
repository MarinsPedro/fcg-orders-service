namespace FiapCloudGames.Orders.Contracts.Events.V1;

public sealed record PurchaseOrderCreatedEvent(
    Guid EventId,
    Guid PurchaseOrderId) : IIntegrationEvent;
