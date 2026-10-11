using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderPoint.Infrastructure.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class Add_DisplayOrder_To_Category : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "Categories",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(
                """
                UPDATE "Categories" AS category
                SET "DisplayOrder" = ordered."Position"
                FROM (
                    SELECT "Id", ROW_NUMBER() OVER (ORDER BY "Name", "Id") AS "Position"
                    FROM "Categories"
                ) AS ordered
                WHERE category."Id" = ordered."Id";
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "Categories");
        }
    }
}
