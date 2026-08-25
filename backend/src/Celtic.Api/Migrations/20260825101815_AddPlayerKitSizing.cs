using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Celtic.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerKitSizing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ShortSize",
                table: "Players",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SockSize",
                table: "Players",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShortSize",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "SockSize",
                table: "Players");
        }
    }
}
