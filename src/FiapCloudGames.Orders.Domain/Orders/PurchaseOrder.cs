namespace FiapCloudGames.Orders.Domain.Orders;

/// <summary>
/// Representa o cabeçalho da ordem de compra.
/// </summary>
public sealed class PurchaseOrder
{
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
        Items = itens;
    }

    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public PurchaseOrderStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? UpdatedAtUtc { get; private set; }

    #region Naviations

    public IEnumerable<PurchaseOrderItem> Items { get; private set; } = Enumerable.Empty<PurchaseOrderItem>();

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
        ValidationStatus(status);
        Status = status;
        UpdatedAtUtc = ValidateUpdatedAt(updatedAtUtc);
    }

    private void ValidationStatus(PurchaseOrderStatus status)
    {
        // TODO: corrigir as regras, incluir a comparação com a situação no banco.
        if (status == PurchaseOrderStatus.OrderCompleted && Status == PurchaseOrderStatus.UserCncelled)
            throw new DomainRuleViolationException($"Não é possível alterar o estado da ordem com situação {PurchaseOrderStatus.OrderCompleted.GetDisplayName()}");

        if (status == PurchaseOrderStatus.Failed)
            throw new DomainRuleViolationException($"Não é possível alterar o estado da ordem com situação {PurchaseOrderStatus.Failed.GetDisplayName()}");

        if (status == PurchaseOrderStatus.UserCncelled && Status == PurchaseOrderStatus.PaymentApproved)
            throw new DomainRuleViolationException($"Não é possível alterar o estado da ordem com situação {PurchaseOrderStatus.UserCncelled.GetDisplayName()}");
    }
}
