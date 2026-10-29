namespace FiapCloudGames.Orders.Infrastructure.Orchestration;

public sealed class PurchaseOrderSagaGame
{
    private PurchaseOrderSagaGame() { }

    private PurchaseOrderSagaGame(
        Guid orderId,
        Guid orderItemId,
        Guid gameId)
    {
        OrderId = orderId;
        OrderItemId = orderItemId;
        GameId = gameId;
        Status = PurchaseOrderSagaGameStatus.WaitingPayment;
    }

    public Guid OrderId { get; private set; }
    public Guid OrderItemId { get; private set; }
    public Guid GameId { get; private set; }
    public PurchaseOrderSagaGameStatus Status { get; private set; }
    public DateTimeOffset? RequestedAtUtc { get; private set; }
    public DateTimeOffset? GrantedAtUtc { get; private set; }

    public static PurchaseOrderSagaGame Create(
        Guid orderId,
        Guid orderItemId,
        Guid gameId) =>
        new(orderId, orderItemId, gameId);

    public void MarkRequested(DateTimeOffset requestedAtUtc)
    {
        if (Status != PurchaseOrderSagaGameStatus.WaitingPayment)
            return;

        Status = PurchaseOrderSagaGameStatus.Requested;
        RequestedAtUtc = requestedAtUtc;
    }

    public bool MarkGranted(DateTimeOffset grantedAtUtc)
    {
        if (Status == PurchaseOrderSagaGameStatus.Granted)
            return false;

        if (Status != PurchaseOrderSagaGameStatus.Requested)
            throw new InvalidOperationException(
                $"O jogo {GameId} da ordem {OrderId} ainda não foi solicitado à biblioteca.");

        Status = PurchaseOrderSagaGameStatus.Granted;
        GrantedAtUtc = grantedAtUtc;
        return true;
    }
}
