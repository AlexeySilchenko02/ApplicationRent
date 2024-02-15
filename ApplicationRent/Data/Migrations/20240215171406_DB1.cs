using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApplicationRent.Data.Migrations
{
    /// <inheritdoc />
    public partial class DB1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "data",
                table: "Place",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "SizePlace",
                schema: "data",
                table: "Place",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.UpdateData(
                schema: "data",
                table: "Place",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "SizePlace" },
                values: new object[] { "Комната 15 метров", 15.5m });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                schema: "data",
                table: "Place");

            migrationBuilder.DropColumn(
                name: "SizePlace",
                schema: "data",
                table: "Place");
        }
    }
}
