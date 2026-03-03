using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexora.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedAvatars : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Avatar_AspNetUsers_UserId",
                table: "Avatar");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Avatar",
                table: "Avatar");

            migrationBuilder.RenameTable(
                name: "Avatar",
                newName: "Avatars");

            migrationBuilder.RenameIndex(
                name: "IX_Avatar_UserId",
                table: "Avatars",
                newName: "IX_Avatars_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Avatars",
                table: "Avatars",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Avatars_AspNetUsers_UserId",
                table: "Avatars",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Avatars_AspNetUsers_UserId",
                table: "Avatars");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Avatars",
                table: "Avatars");

            migrationBuilder.RenameTable(
                name: "Avatars",
                newName: "Avatar");

            migrationBuilder.RenameIndex(
                name: "IX_Avatars_UserId",
                table: "Avatar",
                newName: "IX_Avatar_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Avatar",
                table: "Avatar",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Avatar_AspNetUsers_UserId",
                table: "Avatar",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
