using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Celtic.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerFanShirtAllergies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Allergies",
                table: "Players",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FanNumber",
                table: "Players",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShirtSize",
                table: "Players",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Allergies",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "FanNumber",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "ShirtSize",
                table: "Players");
        }
    }
}
