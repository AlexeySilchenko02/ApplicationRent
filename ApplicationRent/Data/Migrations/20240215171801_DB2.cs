using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApplicationRent.Data.Migrations
{
    /// <inheritdoc />
    public partial class DB2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "SizePlace",
                schema: "data",
                table: "Place",
                type: "float",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.UpdateData(
                schema: "data",
                table: "Place",
                keyColumn: "Id",
                keyValue: 1,
                column: "SizePlace",
                value: 15.5);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "SizePlace",
                schema: "data",
                table: "Place",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.UpdateData(
                schema: "data",
                table: "Place",
                keyColumn: "Id",
                keyValue: 1,
                column: "SizePlace",
                value: 15.5m);
        }
    }
}
