using FiapCloudGames.Orders.Domain.Common;
using FiapCloudGames.Orders.Domain.Orders;

namespace FiapCloudGames.Orders.UnitTests;

public sealed class PurchaseOrderStatusTests
{
    [Fact]
    public void ChangeStatus_ShouldCompleteExpectedSagaFlow()
    {
        var order = CreateOrder();

        order.ChangeStatus(PurchaseOrderStatus.PendingPayment, UtcNow());
        order.ChangeStatus(PurchaseOrderStatus.PaymentApproved, UtcNow());
        order.ChangeStatus(PurchaseOrderStatus.OrderCompleted, UtcNow());

        Assert.Equal(PurchaseOrderStatus.OrderCompleted, order.Status);
    }

    [Fact]
    public void ChangeStatus_ShouldRejectTransitionThatSkipsSagaSteps()
    {
        var order = CreateOrder();

        var exception = Assert.Throws<DomainRuleViolationException>(
            () => order.ChangeStatus(PurchaseOrderStatus.OrderCompleted, UtcNow()));

        Assert.Contains("Não é possível alterar", exception.Message);
        Assert.Equal(PurchaseOrderStatus.Received, order.Status);
    }

    [Fact]
    public void ChangeStatus_ShouldRejectTransitionFromTerminalState()
    {
        var order = CreateOrder();
        order.ChangeStatus(PurchaseOrderStatus.PendingPayment, UtcNow());
        order.ChangeStatus(PurchaseOrderStatus.PaymentRejected, UtcNow());

        Assert.Throws<DomainRuleViolationException>(
            () => order.ChangeStatus(PurchaseOrderStatus.PendingPayment, UtcNow()));
    }

    [Fact]
    public void ChangeStatus_ShouldBeIdempotentForCurrentStatus()
    {
        var order = CreateOrder();

        order.ChangeStatus(PurchaseOrderStatus.Received, UtcNow());

        Assert.Equal(PurchaseOrderStatus.Received, order.Status);
        Assert.Null(order.UpdatedAtUtc);
    }

    private static PurchaseOrder CreateOrder() =>
        PurchaseOrder.Create(
            Guid.NewGuid(),
            [PurchaseOrderItem.Create(Guid.NewGuid(), 50m, 50m, null)],
            UtcNow());

    private static DateTimeOffset UtcNow() =>
        new(2026, 10, 29, 12, 0, 0, TimeSpan.Zero);
}
