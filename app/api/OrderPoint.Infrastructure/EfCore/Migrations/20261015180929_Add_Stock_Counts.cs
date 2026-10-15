using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderPoint.Infrastructure.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class Add_Stock_Counts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StockCounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Groups = table.Column<int[]>(type: "integer[]", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Note = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ClosedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockCounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StockCountLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StockCountId = table.Column<Guid>(type: "uuid", nullable: false),
                    StockItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    CountedQuantity = table.Column<decimal>(type: "numeric(12,3)", precision: 12, scale: 3, nullable: true),
                    ExpectedQuantity = table.Column<decimal>(type: "numeric(12,3)", precision: 12, scale: 3, nullable: true),
                    UnitCost = table.Column<decimal>(type: "numeric(12,6)", precision: 12, scale: 6, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockCountLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockCountLines_StockCounts_StockCountId",
                        column: x => x.StockCountId,
                        principalTable: "StockCounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StockCountLines_StockItems_StockItemId",
                        column: x => x.StockItemId,
                        principalTable: "StockItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StockCountLines_StockCountId_StockItemId",
                table: "StockCountLines",
                columns: new[] { "StockCountId", "StockItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockCountLines_StockItemId",
                table: "StockCountLines",
                column: "StockItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StockCountLines");

            migrationBuilder.DropTable(
                name: "StockCounts");
        }
    }
}
