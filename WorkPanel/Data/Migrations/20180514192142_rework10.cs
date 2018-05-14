using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace WorkPanel.Data.Migrations
{
    public partial class rework10 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Exposure",
                table: "Assets");

            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "Assets",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<double>(
                name: "Rate",
                table: "AssetHistories",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Date",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "Rate",
                table: "AssetHistories");

            migrationBuilder.AddColumn<double>(
                name: "Exposure",
                table: "Assets",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
