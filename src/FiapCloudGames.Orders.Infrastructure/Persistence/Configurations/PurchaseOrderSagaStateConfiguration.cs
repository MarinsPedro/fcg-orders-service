using FiapCloudGames.Orders.Infrastructure.Orchestration;

namespace FiapCloudGames.Orders.Infrastructure.Persistence.Configurations;

internal sealed class PurchaseOrderSagaStateConfiguration :
    IEntityTypeConfiguration<PurchaseOrderSagaState>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderSagaState> builder)
    {
        builder.ToTable("purchase_order_saga_states");
        builder.HasKey(state => state.CorrelationId);

        builder.Property(state => state.CorrelationId)
            .HasColumnName("correlation_id")
            .ValueGeneratedNever();
        builder.Property(state => state.CurrentState)
            .HasColumnName("current_state")
            .HasMaxLength(64)
            .IsRequired();
        builder.Property(state => state.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();
        builder.Property(state => state.PaymentId)
            .HasColumnName("payment_id");
        builder.Property(state => state.TotalAmount)
            .HasColumnName("total_amount")
            .HasPrecision(12, 2)
            .IsRequired();
        builder.Property(state => state.ExpectedGameCount)
            .HasColumnName("expected_game_count")
            .IsRequired();
        builder.Property(state => state.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();
        builder.Property(state => state.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .IsRequired();
        builder.Property(state => state.CompletedAtUtc)
            .HasColumnName("completed_at_utc");

        builder.Ignore(state => state.AllGamesGranted);

        builder.HasIndex(state => new
        {
            state.CurrentState,
            state.CompletedAtUtc
        })
            .HasDatabaseName("ix_purchase_order_saga_states_state_completed");
    }
}
