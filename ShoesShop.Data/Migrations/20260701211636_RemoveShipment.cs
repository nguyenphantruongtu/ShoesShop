using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShoesShop.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveShipment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Shipments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Shipments",
                columns: table => new
                {
                    ShipmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    CarrierName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DeliveredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EstimatedDeliveryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ShippedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ShippingStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "Pending"),
                    TrackingNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Shipment__5CAD37ED5DD16B0E", x => x.ShipmentId);
                    table.ForeignKey(
                        name: "FK_Shipments_Orders",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "UQ__Shipment__C3905BCEAF0FA892",
                table: "Shipments",
                column: "OrderId",
                unique: true);
        }
    }
}
