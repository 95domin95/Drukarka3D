using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Drukarka3DData.Migrations
{
    public partial class NewMigration8 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserFavouriteProject_AspNetUsers_UserIdId",
                table: "UserFavouriteProject");

            migrationBuilder.RenameColumn(
                name: "UserIdId",
                table: "UserFavouriteProject",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserFavouriteProject_UserIdId",
                table: "UserFavouriteProject",
                newName: "IX_UserFavouriteProject_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserFavouriteProject_AspNetUsers_UserId",
                table: "UserFavouriteProject",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserFavouriteProject_AspNetUsers_UserId",
                table: "UserFavouriteProject");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "UserFavouriteProject",
                newName: "UserIdId");

            migrationBuilder.RenameIndex(
                name: "IX_UserFavouriteProject_UserId",
                table: "UserFavouriteProject",
                newName: "IX_UserFavouriteProject_UserIdId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserFavouriteProject_AspNetUsers_UserIdId",
                table: "UserFavouriteProject",
                column: "UserIdId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
