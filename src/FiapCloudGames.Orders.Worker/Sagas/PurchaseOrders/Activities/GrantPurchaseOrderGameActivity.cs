using FiapCloudGames.Orders.Contracts.Events.V1;
using FiapCloudGames.Orders.Domain.Orders;
using FiapCloudGames.Orders.Infrastructure.Orchestration;
using FiapCloudGames.Orders.Infrastructure.Persistence;
using FiapCloudGames.Orders.Worker.Sagas.PurchaseOrders;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace FiapCloudGames.Orders.Worker.Sagas.PurchaseOrders.Activities;

public sealed class GrantPurchaseOrderGameActivity(
    OrdersDbContext dbContext,
    IPublishEndpoint publishEndpoint,
    TimeProvider clock,
    ILogger<GrantPurchaseOrderGameActivity> logger) :
    PurchaseOrderSagaActivity<GameGrantedEventV1>
{
    protected override async Task ExecuteAsync(
        BehaviorContext<PurchaseOrderSagaState, GameGrantedEventV1> context)
    {
        var grantedGame = await dbContext.PurchaseOrderSagaGames
            .SingleOrDefaultAsync(
                game =>
                    game.OrderId == context.Saga.CorrelationId &&
                    game.GameId == context.Message.GameId,
                context.CancellationToken)
            ?? throw new InvalidOperationException(
                $"O jogo {context.Message.GameId} não pertence à ordem {context.Saga.CorrelationId}.");

        var grantedNow = grantedGame.MarkGranted(
            context.Message.LibraryEntryId,
            context.Message.GrantedAtUtc);

        var persistedGrantedCount = await dbContext.PurchaseOrderSagaGames
            .CountAsync(
                game =>
                    game.OrderId == context.Saga.CorrelationId &&
                    game.Status == PurchaseOrderSagaGameStatus.Granted,
                context.CancellationToken);
        var effectiveGrantedCount =
            persistedGrantedCount + (grantedNow ? 1 : 0);

        if (effectiveGrantedCount > context.Saga.ExpectedGameCount)
            throw new InvalidOperationException(
                $"A saga da ordem {context.Saga.CorrelationId} possui mais jogos concedidos que o esperado.");

        var now = clock.GetUtcNow();
        context.Saga.AllGamesGranted =
            effectiveGrantedCount == context.Saga.ExpectedGameCount;
        context.Saga.UpdatedAtUtc = now;

        if (!context.Saga.AllGamesGranted)
            return;

        var order = await dbContext.PurchaseOrders.SingleOrDefaultAsync(
            purchaseOrder => purchaseOrder.Id == context.Saga.CorrelationId,
            context.CancellationToken)
            ?? throw new InvalidOperationException(
                $"A ordem {context.Saga.CorrelationId} não foi encontrada.");

        var reservations = await dbContext.PurchaseOrderGameReservations
            .Where(reservation =>
                reservation.PurchaseOrderId == context.Saga.CorrelationId)
            .ToArrayAsync(context.CancellationToken);

        order.ChangeStatus(PurchaseOrderStatus.OrderCompleted, now);
        dbContext.PurchaseOrderGameReservations.RemoveRange(reservations);
        context.Saga.CompletedAtUtc = now;

        await publishEndpoint.Publish(
            new OrderCompletedEventV1(
                Guid.NewGuid(),
                order.Id,
                order.CustomerId,
                now),
            publishContext =>
            {
                publishContext.MessageId = publishContext.Message.EventId;
                publishContext.CorrelationId = order.Id;
            },
            context.CancellationToken);

        logger.LogInformation(
            "Todos os {GameCount} jogos foram concedidos; ordem {OrderId} concluída.",
            context.Saga.ExpectedGameCount,
            order.Id);
    }
}
