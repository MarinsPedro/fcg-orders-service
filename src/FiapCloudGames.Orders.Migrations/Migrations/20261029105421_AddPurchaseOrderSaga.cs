using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FiapCloudGames.Orders.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseOrderSaga : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "purchase_order_game_reservations",
                schema: "orders",
                columns: table => new
                {
                    purchase_order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    game_id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_purchase_order_game_reservations", x => new { x.purchase_order_id, x.game_id });
                    table.ForeignKey(
                        name: "FK_purchase_order_game_reservations_purchase_orders_purchase_o~",
                        column: x => x.purchase_order_id,
                        principalSchema: "orders",
                        principalTable: "purchase_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "purchase_order_saga_states",
                schema: "orders",
                columns: table => new
                {
                    correlation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    current_state = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_id = table.Column<Guid>(type: "uuid", nullable: true),
                    total_amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    expected_game_count = table.Column<int>(type: "integer", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    completed_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_purchase_order_saga_states", x => x.correlation_id);
                });

            migrationBuilder.CreateTable(
                name: "purchase_order_saga_games",
                schema: "orders",
                columns: table => new
                {
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    game_id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    library_entry_id = table.Column<Guid>(type: "uuid", nullable: true),
                    requested_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    granted_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_purchase_order_saga_games", x => new { x.order_id, x.game_id });
                    table.ForeignKey(
                        name: "FK_purchase_order_saga_games_purchase_order_saga_states_order_~",
                        column: x => x.order_id,
                        principalSchema: "orders",
                        principalTable: "purchase_order_saga_states",
                        principalColumn: "correlation_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ux_purchase_order_game_reservations_customer_game",
                schema: "orders",
                table: "purchase_order_game_reservations",
                columns: new[] { "customer_id", "game_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_purchase_order_saga_games_order_status",
                schema: "orders",
                table: "purchase_order_saga_games",
                columns: new[] { "order_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ux_purchase_order_saga_games_order_item",
                schema: "orders",
                table: "purchase_order_saga_games",
                column: "order_item_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_purchase_order_saga_states_state_completed",
                schema: "orders",
                table: "purchase_order_saga_states",
                columns: new[] { "current_state", "completed_at_utc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "purchase_order_game_reservations",
                schema: "orders");

            migrationBuilder.DropTable(
                name: "purchase_order_saga_games",
                schema: "orders");

            migrationBuilder.DropTable(
                name: "purchase_order_saga_states",
                schema: "orders");
        }
    }
}
