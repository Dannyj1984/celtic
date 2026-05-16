using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Celtic.Api.Migrations
{
    /// <inheritdoc />
    public partial class goodToKnow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GoodToKnow",
                table: "ClubSettings",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GoodToKnow",
                table: "ClubSettings");
        }
    }
}
