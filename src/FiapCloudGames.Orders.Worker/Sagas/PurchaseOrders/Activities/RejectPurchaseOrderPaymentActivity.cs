using FiapCloudGames.Orders.Contracts.Events.V1;
using FiapCloudGames.Orders.Domain.Orders;
using FiapCloudGames.Orders.Infrastructure.Orchestration;
using FiapCloudGames.Orders.Infrastructure.Persistence;
using FiapCloudGames.Orders.Worker.Sagas.PurchaseOrders;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace FiapCloudGames.Orders.Worker.Sagas.PurchaseOrders.Activities;

public sealed class RejectPurchaseOrderPaymentActivity(
    OrdersDbContext dbContext,
    TimeProvider clock,
    ILogger<RejectPurchaseOrderPaymentActivity> logger) :
    PurchaseOrderSagaActivity<PaymentRejectedEventV1>
{
    protected override async Task ExecuteAsync(
        BehaviorContext<PurchaseOrderSagaState, PaymentRejectedEventV1> context)
    {
        var order = await dbContext.PurchaseOrders.SingleOrDefaultAsync(
            purchaseOrder => purchaseOrder.Id == context.Saga.CorrelationId,
            context.CancellationToken)
            ?? throw new InvalidOperationException(
                $"A ordem {context.Saga.CorrelationId} não foi encontrada.");

        var reservations = await dbContext.PurchaseOrderGameReservations
            .Where(reservation =>
                reservation.PurchaseOrderId == context.Saga.CorrelationId)
            .ToArrayAsync(context.CancellationToken);

        var now = clock.GetUtcNow();
        order.ChangeStatus(PurchaseOrderStatus.PaymentRejected, now);
        dbContext.PurchaseOrderGameReservations.RemoveRange(reservations);

        context.Saga.PaymentId = context.Message.PaymentId;
        context.Saga.UpdatedAtUtc = now;
        context.Saga.CompletedAtUtc = now;

        logger.LogWarning(
            "Pagamento {PaymentId} rejeitado para a ordem {OrderId}. Motivo: {RejectionReason}",
            context.Message.PaymentId,
            context.Saga.CorrelationId,
            context.Message.RejectionReason);
    }
}
