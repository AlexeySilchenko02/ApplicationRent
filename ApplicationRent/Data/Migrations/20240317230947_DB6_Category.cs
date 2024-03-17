using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApplicationRent.Data.Migrations
{
    /// <inheritdoc />
    public partial class DB6_Category : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                schema: "data",
                table: "Place",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                schema: "data",
                table: "Place",
                keyColumn: "Id",
                keyValue: 1,
                column: "Category",
                value: "Склад");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                schema: "data",
                table: "Place");
        }
    }
}
