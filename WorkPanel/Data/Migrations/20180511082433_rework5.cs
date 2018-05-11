using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace WorkPanel.Data.Migrations
{
    public partial class rework5 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PurchaseQuantity",
                table: "Assets");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "PurchaseQuantity",
                table: "Assets",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
