namespace FiapCloudGames.Orders.Worker.Messaging;

// DTO de entrada (JSON puro) para PaymentPaidEventV1 e PaymentFailedEventV1.
// A exchange recebe ambos; ProviderTransactionId ou FailureReason discrimina o resultado.
public sealed class PaymentOutcomeMessage
{
    public Guid PaymentId { get; set; }
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public string? ProviderTransactionId { get; set; }
    public string? FailureReason { get; set; }
    public DateTimeOffset OccurredAtUtc { get; set; }
}
