using FiapCloudGames.Orders.Contracts.Events.V1;
using FiapCloudGames.Orders.Domain.Orders;
using FiapCloudGames.Orders.Infrastructure.Orchestration;
using FiapCloudGames.Orders.Infrastructure.Persistence;
using FiapCloudGames.Orders.Worker.Sagas.PurchaseOrders;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace FiapCloudGames.Orders.Worker.Sagas.PurchaseOrders.Activities;

public sealed class InitializePurchaseOrderActivity(
    OrdersDbContext dbContext,
    IPublishEndpoint publishEndpoint,
    TimeProvider clock,
    ILogger<InitializePurchaseOrderActivity> logger) :
    PurchaseOrderSagaActivity<PurchaseOrderCreatedEvent>
{
    protected override async Task ExecuteAsync(
        BehaviorContext<PurchaseOrderSagaState, PurchaseOrderCreatedEvent> context)
    {
        var order = await dbContext.PurchaseOrders
            .Include(purchaseOrder => purchaseOrder.Items)
            .SingleOrDefaultAsync(
                purchaseOrder => purchaseOrder.Id == context.Message.PurchaseOrderId,
                context.CancellationToken)
            ?? throw new InvalidOperationException(
                $"A ordem {context.Message.PurchaseOrderId} não foi encontrada.");

        var items = order.Items.ToArray();
        if (items.Length == 0)
            throw new InvalidOperationException(
                $"A ordem {order.Id} não possui jogos para processar.");

        var now = clock.GetUtcNow();
        var totalAmount = items.Sum(item => item.FinalPrice.Amount);

        context.Saga.CustomerId = order.CustomerId;
        context.Saga.TotalAmount = totalAmount;
        context.Saga.ExpectedGameCount = items.Length;
        context.Saga.CreatedAtUtc = now;
        context.Saga.UpdatedAtUtc = now;

        dbContext.PurchaseOrderSagaGames.AddRange(items.Select(item =>
            PurchaseOrderSagaGame.Create(order.Id, item.Id, item.ProductId)));

        order.ChangeStatus(PurchaseOrderStatus.PendingPayment, now);

        await publishEndpoint.Publish(
            new PaymentRequestedEventV1(
                Guid.NewGuid(),
                order.Id,
                order.CustomerId,
                totalAmount,
                now),
            publishContext =>
            {
                publishContext.MessageId = publishContext.Message.EventId;
                publishContext.CorrelationId = order.Id;
            },
            context.CancellationToken);

        logger.LogInformation(
            "Saga da ordem {OrderId} iniciada com {GameCount} jogos; pagamento solicitado.",
            order.Id,
            items.Length);
    }
}
