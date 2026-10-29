using FiapCloudGames.Orders.Contracts.Events.V1;
using FiapCloudGames.Orders.Domain.Orders;
using FiapCloudGames.Orders.Infrastructure.Orchestration;
using FiapCloudGames.Orders.Infrastructure.Persistence;
using FiapCloudGames.Orders.Worker.Sagas.PurchaseOrders;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace FiapCloudGames.Orders.Worker.Sagas.PurchaseOrders.Activities;

public sealed class ApprovePurchaseOrderPaymentActivity(
    OrdersDbContext dbContext,
    IPublishEndpoint publishEndpoint,
    TimeProvider clock,
    ILogger<ApprovePurchaseOrderPaymentActivity> logger) :
    PurchaseOrderSagaActivity<PaymentApprovedEventV1>
{
    protected override async Task ExecuteAsync(
        BehaviorContext<PurchaseOrderSagaState, PaymentApprovedEventV1> context)
    {
        var order = await dbContext.PurchaseOrders.SingleOrDefaultAsync(
            purchaseOrder => purchaseOrder.Id == context.Saga.CorrelationId,
            context.CancellationToken)
            ?? throw new InvalidOperationException(
                $"A ordem {context.Saga.CorrelationId} não foi encontrada.");

        var games = await dbContext.PurchaseOrderSagaGames
            .Where(game => game.OrderId == context.Saga.CorrelationId)
            .ToArrayAsync(context.CancellationToken);

        if (games.Length != context.Saga.ExpectedGameCount)
            throw new InvalidOperationException(
                $"A saga da ordem {context.Saga.CorrelationId} possui uma quantidade inválida de jogos.");

        var now = clock.GetUtcNow();
        order.ChangeStatus(PurchaseOrderStatus.PaymentApproved, now);
        context.Saga.UpdatedAtUtc = now;

        foreach (var game in games)
        {
            game.MarkRequested(now);

            await publishEndpoint.Publish(
                new GameGrantRequestedEventV1(
                    Guid.NewGuid(),
                    context.Saga.CorrelationId,
                    context.Saga.CustomerId,
                    game.GameId,
                    now),
                publishContext =>
                {
                    publishContext.MessageId = publishContext.Message.EventId;
                    publishContext.CorrelationId = context.Saga.CorrelationId;
                },
                context.CancellationToken);
        }

        logger.LogInformation(
            "Pagamento aprovado para a ordem {OrderId}; {GameCount} concessões solicitadas.",
            context.Saga.CorrelationId,
            games.Length);
    }
}
