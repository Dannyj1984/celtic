using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Celtic.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddTrainingSessionCaptains : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "Captain1PlayerId",
                table: "Events",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Captain2PlayerId",
                table: "Events",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_Captain1PlayerId",
                table: "Events",
                column: "Captain1PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_Captain2PlayerId",
                table: "Events",
                column: "Captain2PlayerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Players_Captain1PlayerId",
                table: "Events",
                column: "Captain1PlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Players_Captain2PlayerId",
                table: "Events",
                column: "Captain2PlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_Players_Captain1PlayerId",
                table: "Events");

            migrationBuilder.DropForeignKey(
                name: "FK_Events_Players_Captain2PlayerId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_Captain1PlayerId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_Captain2PlayerId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "Captain1PlayerId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "Captain2PlayerId",
                table: "Events");
        }
    }
}
