namespace FiapCloudGames.Orders.Domain.Orders;

/// <summary>
/// Representa o cabeçalho da ordem de compra.
/// </summary>
public sealed class PurchaseOrder
{
    private readonly List<PurchaseOrderItem> _items = [];

    public PurchaseOrder() { }

    private PurchaseOrder(
        Guid id,
        Guid customerId,
        IEnumerable<PurchaseOrderItem> itens,
        DateTimeOffset createdAtUtc)
    {
        Id = id;
        CustomerId = customerId;
        Status = PurchaseOrderStatus.Received;
        CreatedAtUtc = ValidateCreatedAt(createdAtUtc);
        _items.AddRange(itens);
    }

    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public PurchaseOrderStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? UpdatedAtUtc { get; private set; }

    #region Naviations

    public IReadOnlyCollection<PurchaseOrderItem> Items => _items;

    #endregion

    public static PurchaseOrder Create(
        Guid customerId,
        IEnumerable<PurchaseOrderItem> itens,
        DateTimeOffset createdAtUtc)
    {
        return new PurchaseOrder(Guid.NewGuid(), customerId, itens, createdAtUtc);
    }

    private static DateTimeOffset ValidateCreatedAt(
        DateTimeOffset createdAtUtc) =>
        createdAtUtc != default && createdAtUtc.Offset == TimeSpan.Zero
            ? createdAtUtc
            : throw new DomainRuleViolationException(
                "A data de criação da ordem de compra deve estar em UTC.");

    private static DateTimeOffset ValidateUpdatedAt(
        DateTimeOffset updatedUtc) =>
        updatedUtc != default && updatedUtc.Offset == TimeSpan.Zero
            ? updatedUtc
            : throw new DomainRuleViolationException("A data de atualização da ordem de compra deve estar em UTC.");

    public void ChangeStatus(PurchaseOrderStatus status, DateTimeOffset updatedAtUtc)
    {
        if (Status == status)
            return;

        ValidateStatusTransition(status);
        Status = status;
        UpdatedAtUtc = ValidateUpdatedAt(updatedAtUtc);
    }

    private void ValidateStatusTransition(PurchaseOrderStatus nextStatus)
    {
        var isAllowed = Status switch
        {
            PurchaseOrderStatus.Received => nextStatus is
                PurchaseOrderStatus.PendingPayment or
                PurchaseOrderStatus.UserCncelled or
                PurchaseOrderStatus.Failed,
            PurchaseOrderStatus.PendingPayment => nextStatus is
                PurchaseOrderStatus.PaymentApproved or
                PurchaseOrderStatus.PaymentRejected or
                PurchaseOrderStatus.UserCncelled or
                PurchaseOrderStatus.Failed,
            PurchaseOrderStatus.PaymentApproved => nextStatus is
                PurchaseOrderStatus.OrderCompleted or
                PurchaseOrderStatus.Failed,
            _ => false
        };

        if (!isAllowed)
            throw new DomainRuleViolationException(
                $"Não é possível alterar a ordem de {Status.GetDisplayName()} para {nextStatus.GetDisplayName()}.");
    }
}
