using MassTransit;

namespace FiapCloudGames.Orders.Infrastructure.Orchestration;

public sealed class PurchaseOrderSagaState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public Guid? PaymentId { get; set; }
    public decimal TotalAmount { get; set; }
    public int ExpectedGameCount { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public DateTimeOffset? CompletedAtUtc { get; set; }
    public bool AllGamesGranted { get; set; }
}
