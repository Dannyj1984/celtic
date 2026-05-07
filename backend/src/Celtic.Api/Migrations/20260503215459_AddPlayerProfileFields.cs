using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Celtic.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerProfileFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PreferredFoot",
                table: "Players",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "PlayerOfTheMatchId",
                table: "Matches",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Matches_PlayerOfTheMatchId",
                table: "Matches",
                column: "PlayerOfTheMatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_Players_PlayerOfTheMatchId",
                table: "Matches",
                column: "PlayerOfTheMatchId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Matches_Players_PlayerOfTheMatchId",
                table: "Matches");

            migrationBuilder.DropIndex(
                name: "IX_Matches_PlayerOfTheMatchId",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "PreferredFoot",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "PlayerOfTheMatchId",
                table: "Matches");
        }
    }
}
