using FiapCloudGames.Orders.Contracts.Events.V1;
using MassTransit;

namespace FiapCloudGames.Orders.Worker.Messaging;

// Traduz o retorno do Library para o contrato interno de PurchaseOrderStateMachine.
public sealed class LibraryGrantConsumer(
    IPublishEndpoint publishEndpoint,
    ILogger<LibraryGrantConsumer> logger) : IConsumer<LibraryGrantMessage>
{
    public async Task Consume(ConsumeContext<LibraryGrantMessage> context)
    {
        var grant = context.Message;
        if (grant.LibraryEntryId == Guid.Empty ||
            grant.OrderId == Guid.Empty ||
            grant.UserId == Guid.Empty ||
            grant.GameId == Guid.Empty)
            throw new InvalidOperationException("GameGranted externo contem identificadores invalidos.");

        var eventId = Guid.NewGuid();
        await publishEndpoint.Publish(
            new GameGrantedEventV1(
                eventId, grant.OrderId, grant.UserId,
                grant.GameId, grant.GrantedAtUtc),
            send =>
            {
                send.MessageId = eventId;
                send.CorrelationId = grant.OrderId;
            },
            context.CancellationToken);

        logger.LogInformation(
            "Library granted -> Orders GameGranted. OrderId={OrderId}, GameId={GameId}, EntryId={EntryId}",
            grant.OrderId, grant.GameId, grant.LibraryEntryId);
    }
}
