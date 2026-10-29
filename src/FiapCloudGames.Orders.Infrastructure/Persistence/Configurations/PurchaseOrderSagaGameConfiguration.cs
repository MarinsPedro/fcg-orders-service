using FiapCloudGames.Orders.Infrastructure.Orchestration;

namespace FiapCloudGames.Orders.Infrastructure.Persistence.Configurations;

internal sealed class PurchaseOrderSagaGameConfiguration :
    IEntityTypeConfiguration<PurchaseOrderSagaGame>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderSagaGame> builder)
    {
        builder.ToTable("purchase_order_saga_games");
        builder.HasKey(game => new { game.OrderId, game.GameId });

        builder.Property(game => game.OrderId)
            .HasColumnName("order_id");
        builder.Property(game => game.OrderItemId)
            .HasColumnName("order_item_id")
            .IsRequired();
        builder.Property(game => game.GameId)
            .HasColumnName("game_id");
        builder.Property(game => game.Status)
            .HasColumnName("status")
            .HasConversion<short>()
            .IsRequired();
        builder.Property(game => game.RequestedAtUtc)
            .HasColumnName("requested_at_utc");
        builder.Property(game => game.GrantedAtUtc)
            .HasColumnName("granted_at_utc");

        builder.HasIndex(game => game.OrderItemId)
            .IsUnique()
            .HasDatabaseName("ux_purchase_order_saga_games_order_item");

        builder.HasIndex(game => new { game.OrderId, game.Status })
            .HasDatabaseName("ix_purchase_order_saga_games_order_status");

        builder.HasOne<PurchaseOrderSagaState>()
            .WithMany()
            .HasForeignKey(game => game.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
