using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApplicationRent.Data.Migrations
{
    /// <inheritdoc />
    public partial class DB8_ImageURL : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "data",
                table: "Place",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl1",
                schema: "data",
                table: "Place",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl2",
                schema: "data",
                table: "Place",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl3",
                schema: "data",
                table: "Place",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl1",
                schema: "data",
                table: "Place");

            migrationBuilder.DropColumn(
                name: "ImageUrl2",
                schema: "data",
                table: "Place");

            migrationBuilder.DropColumn(
                name: "ImageUrl3",
                schema: "data",
                table: "Place");

            migrationBuilder.InsertData(
                schema: "data",
                table: "Place",
                columns: new[] { "Id", "Category", "Description", "EndRent", "InRent", "Name", "Price", "SizePlace", "StartRent" },
                values: new object[] { 1, "Склад", "Комната 15 метров", new DateTime(2024, 2, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "First", 1500.500m, 15.5, new DateTime(2024, 2, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });
        }
    }
}
