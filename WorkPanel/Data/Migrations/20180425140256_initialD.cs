using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace WorkPanel.Data.Migrations
{
    public partial class initialD : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "JsonDateList",
                table: "Investors",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JsonDeactivateDateList",
                table: "Investors",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JsonDateList",
                table: "Investors");

            migrationBuilder.DropColumn(
                name: "JsonDeactivateDateList",
                table: "Investors");
        }
    }
}
