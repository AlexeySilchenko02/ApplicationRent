using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApplicationRent.Data.Migrations
{
    /// <inheritdoc />
    public partial class Newtableplace : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "data");

            migrationBuilder.CreateTable(
                name: "Place",
                schema: "data",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StartRent = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndRent = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InRent = table.Column<bool>(type: "bit", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Place", x => x.Id);
                });

            migrationBuilder.InsertData(
                schema: "data",
                table: "Place",
                columns: new[] { "Id", "EndRent", "InRent", "Name", "Price", "StartRent" },
                values: new object[] { 1, new DateTime(2024, 2, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "First", 1500.500m, new DateTime(2024, 2, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Place",
                schema: "data");
        }
    }
}
