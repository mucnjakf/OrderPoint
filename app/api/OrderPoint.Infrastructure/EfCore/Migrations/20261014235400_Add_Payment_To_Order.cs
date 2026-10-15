using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderPoint.Infrastructure.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class Add_Payment_To_Order : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PaidAtUtc",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentMethod",
                table: "Orders",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentStatus",
                table: "Orders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "RefundedAtUtc",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);

            // Orders placed before payments existed count as paid by card when placed (PaymentMethod 1 = Card);
            // declined (Status 2) and cancelled (5) ones as refunded then (PaymentStatus 3), the rest as paid (1)
            migrationBuilder.Sql(
                """
                UPDATE "Orders"
                SET
                    "PaymentMethod" = 1,
                    "PaidAtUtc" = "CreatedAtUtc",
                    "PaymentStatus" = CASE WHEN "Status" IN (2, 5) THEN 3 ELSE 1 END,
                    "RefundedAtUtc" = CASE "Status"
                        WHEN 2 THEN "DeclinedAtUtc"
                        WHEN 5 THEN "CancelledAtUtc"
                    END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaidAtUtc",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "RefundedAtUtc",
                table: "Orders");
        }
    }
}
