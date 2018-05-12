using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Drukarka3DData.Migrations
{
    public partial class NewMigration7 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "isRated",
                table: "UserFavouriteProject",
                newName: "IsRated");

            migrationBuilder.AddColumn<int>(
                name: "OrderId",
                table: "UserFavouriteProject",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserFavouriteProject_OrderId",
                table: "UserFavouriteProject",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserFavouriteProject_Order_OrderId",
                table: "UserFavouriteProject",
                column: "OrderId",
                principalTable: "Order",
                principalColumn: "OrderId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserFavouriteProject_Order_OrderId",
                table: "UserFavouriteProject");

            migrationBuilder.DropIndex(
                name: "IX_UserFavouriteProject_OrderId",
                table: "UserFavouriteProject");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "UserFavouriteProject");

            migrationBuilder.RenameColumn(
                name: "IsRated",
                table: "UserFavouriteProject",
                newName: "isRated");
        }
    }
}
