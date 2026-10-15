namespace FiapCloudGames.Orders.Domain.Orders;

/// <summary>
/// Representa os itens do pedido de compra.
/// </summary>
public class PurchaseOrderItem
{
    public PurchaseOrderItem() { }

    private PurchaseOrderItem(
        Guid id,
        Guid productId,
        decimal basePrice,
        decimal finalPrice,
        Guid? promotionId)
    {
        Id = id;
        ProductId = productId;
        BasePrice = PurchasePrice.Create(basePrice);
        FinalPrice = PurchasePrice.Create(finalPrice);
        PromotionId = promotionId;
    }

    /// <summary>
    /// O ID do item para a compra.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// O ID da compra.
    /// </summary>
    public Guid PurchaseOrderId { get; private set; }

    /// <summary>
    /// O ID do produto, no caso o jogo.
    /// </summary>
    public Guid ProductId { get; private set; }

    /// <summary>
    /// O preço do produto (o jogo) no ato da compra.
    /// </summary>
    public PurchasePrice BasePrice { get; private set; }

    /// <summary>
    /// Preço final do jogo no ato da compra.
    /// </summary>
    public PurchasePrice FinalPrice { get; private set; }

    /// <summary>
    /// O ID da promoção que pode ser informada ou não.
    /// </summary>
    public Guid? PromotionId { get; private set; }

    public static PurchaseOrderItem Create(
        Guid productId,
        decimal basePrice,
        decimal finalPrice,
        Guid? promotionId)
    {
        return new(Guid.NewGuid(), productId, basePrice, finalPrice, promotionId);
    }

    public void BindItemsToPurchaseOrder(Guid purchaseOrderId)
    {
        PurchaseOrderId = purchaseOrderId;
    }

    #region Navigations

    public PurchaseOrder PurchaseOrder { get; private set; } = new();

    #endregion
}
