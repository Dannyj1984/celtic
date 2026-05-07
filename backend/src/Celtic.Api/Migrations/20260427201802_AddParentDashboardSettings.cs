using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Celtic.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddParentDashboardSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SubscriptionStatus",
                table: "Players",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ClubSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NextSubPaymentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TrainingDay = table.Column<int>(type: "integer", nullable: false),
                    TrainingStartTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    TrainingEndTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    TrainingLocation = table.Column<string>(type: "text", nullable: false),
                    CoachWhatsAppNumber = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClubSettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClubSettings");

            migrationBuilder.DropColumn(
                name: "SubscriptionStatus",
                table: "Players");
        }
    }
}
