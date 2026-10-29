namespace FiapCloudGames.Orders.Infrastructure.Persistence.Configurations;

internal sealed class PurchaseOrderGameReservationConfiguration :
    IEntityTypeConfiguration<PurchaseOrderGameReservation>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderGameReservation> builder)
    {
        builder.ToTable("purchase_order_game_reservations");

        builder.HasKey(reservation => new
        {
            reservation.PurchaseOrderId,
            reservation.GameId
        });

        builder.Property(reservation => reservation.PurchaseOrderId)
            .HasColumnName("purchase_order_id");
        builder.Property(reservation => reservation.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();
        builder.Property(reservation => reservation.GameId)
            .HasColumnName("game_id");
        builder.Property(reservation => reservation.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.HasIndex(reservation => new
        {
            reservation.CustomerId,
            reservation.GameId
        })
            .IsUnique()
            .HasDatabaseName("ux_purchase_order_game_reservations_customer_game");

        builder.HasOne<PurchaseOrder>()
            .WithMany()
            .HasForeignKey(reservation => reservation.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
