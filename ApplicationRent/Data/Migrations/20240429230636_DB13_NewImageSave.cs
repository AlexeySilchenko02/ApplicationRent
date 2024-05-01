using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApplicationRent.Data.Migrations
{
    /// <inheritdoc />
    public partial class DB13_NewImageSave : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl1",
                schema: "data",
                table: "Place");

            migrationBuilder.RenameColumn(
                name: "ImageUrl3",
                schema: "data",
                table: "Place",
                newName: "ImageFileName3");

            migrationBuilder.RenameColumn(
                name: "ImageUrl2",
                schema: "data",
                table: "Place",
                newName: "ImageFileName2");

            migrationBuilder.AddColumn<string>(
                name: "ImageFileName1",
                schema: "data",
                table: "Place",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageFileName1",
                schema: "data",
                table: "Place");

            migrationBuilder.RenameColumn(
                name: "ImageFileName3",
                schema: "data",
                table: "Place",
                newName: "ImageUrl3");

            migrationBuilder.RenameColumn(
                name: "ImageFileName2",
                schema: "data",
                table: "Place",
                newName: "ImageUrl2");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl1",
                schema: "data",
                table: "Place",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
