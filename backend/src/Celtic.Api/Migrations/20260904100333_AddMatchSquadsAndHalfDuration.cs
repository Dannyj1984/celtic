using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Celtic.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchSquadsAndHalfDuration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HalfDurationMinutes",
                table: "Matches",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "MatchSquads",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: true),
                    EventId = table.Column<Guid>(type: "uuid", nullable: true),
                    HalfDurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    TotalPeriods = table.Column<int>(type: "integer", nullable: false),
                    PeriodDurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    FirstHalfGoalkeeperPlayerId = table.Column<Guid>(type: "uuid", nullable: true),
                    SecondHalfGoalkeeperPlayerId = table.Column<Guid>(type: "uuid", nullable: true),
                    SquadDataJson = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchSquads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatchSquads_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MatchSquads_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MatchSquads_Players_FirstHalfGoalkeeperPlayerId",
                        column: x => x.FirstHalfGoalkeeperPlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MatchSquads_Players_SecondHalfGoalkeeperPlayerId",
                        column: x => x.SecondHalfGoalkeeperPlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MatchSquads_EventId",
                table: "MatchSquads",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchSquads_FirstHalfGoalkeeperPlayerId",
                table: "MatchSquads",
                column: "FirstHalfGoalkeeperPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchSquads_MatchId",
                table: "MatchSquads",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchSquads_SecondHalfGoalkeeperPlayerId",
                table: "MatchSquads",
                column: "SecondHalfGoalkeeperPlayerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MatchSquads");

            migrationBuilder.DropColumn(
                name: "HalfDurationMinutes",
                table: "Matches");
        }
    }
}
