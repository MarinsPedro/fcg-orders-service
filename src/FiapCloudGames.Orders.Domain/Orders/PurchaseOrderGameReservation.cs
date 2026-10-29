namespace FiapCloudGames.Orders.Domain.Orders;

/// <summary>
/// Reserva um jogo enquanto existe uma compra não finalizada para o cliente.
/// </summary>
public sealed class PurchaseOrderGameReservation
{
    private PurchaseOrderGameReservation() { }

    private PurchaseOrderGameReservation(
        Guid purchaseOrderId,
        Guid customerId,
        Guid gameId,
        DateTimeOffset createdAtUtc)
    {
        PurchaseOrderId = purchaseOrderId;
        CustomerId = customerId;
        GameId = gameId;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid PurchaseOrderId { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid GameId { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }

    public static PurchaseOrderGameReservation Create(
        Guid purchaseOrderId,
        Guid customerId,
        Guid gameId,
        DateTimeOffset createdAtUtc) =>
        new(purchaseOrderId, customerId, gameId, createdAtUtc);
}
