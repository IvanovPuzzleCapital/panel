using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace WorkPanel.Data.Migrations
{
    public partial class rework7 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssetHistories_Assets_AssetId",
                table: "AssetHistories");

            migrationBuilder.DropIndex(
                name: "IX_AssetHistories_AssetId",
                table: "AssetHistories");

            migrationBuilder.DropColumn(
                name: "AssetId",
                table: "AssetHistories");

            migrationBuilder.AddColumn<string>(
                name: "ShortName",
                table: "AssetHistories",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShortName",
                table: "AssetHistories");

            migrationBuilder.AddColumn<int>(
                name: "AssetId",
                table: "AssetHistories",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssetHistories_AssetId",
                table: "AssetHistories",
                column: "AssetId");

            migrationBuilder.AddForeignKey(
                name: "FK_AssetHistories_Assets_AssetId",
                table: "AssetHistories",
                column: "AssetId",
                principalTable: "Assets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
