﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApplicationRent.Data.Migrations
{
    /// <inheritdoc />
    public partial class DB9_RequestsRents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RequestsRents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PlaceId = table.Column<int>(type: "int", nullable: false),
                    StartRent = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndRent = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserPhone = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestsRents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequestsRents_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RequestsRents_Place_PlaceId",
                        column: x => x.PlaceId,
                        principalSchema: "data",
                        principalTable: "Place",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RequestsRents_PlaceId",
                table: "RequestsRents",
                column: "PlaceId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestsRents_UserId",
                table: "RequestsRents",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RequestsRents");
        }
    }
}