namespace FiapCloudGames.Orders.Infrastructure.Persistence.Configurations;

internal sealed class PurchaseOrderItemConfigurations : IEntityTypeConfiguration<PurchaseOrderItem>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderItem> builder)
    {
        var priceConverter =
            new ValueConverter<PurchasePrice, decimal>(
                price => price.Amount,
                value => PurchasePrice.Create(value));

        builder.ToTable("purchase_order_items");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.PurchaseOrderId).HasColumnName("purchase_ordem_id").IsRequired();
        builder.Property(x => x.ProductId).HasColumnName("product_id").IsRequired();

        builder.Property(x => x.BasePrice)
            .HasColumnName("base_price")
            .HasConversion(priceConverter)
            .HasPrecision(12, 2)
            .IsRequired();

        builder.Property(x => x.FinalPrice)
            .HasColumnName("final_price")
            .HasConversion(priceConverter)
            .HasPrecision(12, 2)
            .IsRequired();

        builder.Property(x => x.PromotionId).HasColumnName("promotion_id").IsRequired(false);

        #region Foreing key setup

        builder
            .HasOne(poi => poi.PurchaseOrder)
            .WithMany(po => po.Items)
            .HasForeignKey(poi => poi.PurchaseOrderId)
            .HasConstraintName("fk_purchase_order_purchase_order_items_purchase_order_id") // Padrão da FK = fk_<nome_tabela_pai>_<nome_tabela_filha>_coluna(s)_que_são_fk na relação.
            .OnDelete(DeleteBehavior.NoAction);

        #endregion
    }
}
