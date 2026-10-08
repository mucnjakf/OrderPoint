using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderPoint.Infrastructure.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class Add_Bartender_To_Order : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BartenderId",
                table: "Orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_BartenderId",
                table: "Orders",
                column: "BartenderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Bartenders_BartenderId",
                table: "Orders",
                column: "BartenderId",
                principalTable: "Bartenders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Bartenders_BartenderId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_BartenderId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "BartenderId",
                table: "Orders");
        }
    }
}
