using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderPoint.Infrastructure.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class Add_Register_Payment_To_Order : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CashReceived",
                table: "Orders",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PaidByBartenderId",
                table: "Orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_PaidByBartenderId",
                table: "Orders",
                column: "PaidByBartenderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Bartenders_PaidByBartenderId",
                table: "Orders",
                column: "PaidByBartenderId",
                principalTable: "Bartenders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Bartenders_PaidByBartenderId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_PaidByBartenderId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CashReceived",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PaidByBartenderId",
                table: "Orders");
        }
    }
}
