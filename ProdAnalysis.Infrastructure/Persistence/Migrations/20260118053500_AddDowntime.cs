using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProdAnalysis.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDowntime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DowntimeReasons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DowntimeReasons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HourlyDowntimes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    HourlyRecordId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DowntimeReasonId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Minutes = table.Column<int>(type: "INTEGER", nullable: false),
                    Comment = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HourlyDowntimes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HourlyDowntimes_DowntimeReasons_DowntimeReasonId",
                        column: x => x.DowntimeReasonId,
                        principalTable: "DowntimeReasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HourlyDowntimes_HourlyRecords_HourlyRecordId",
                        column: x => x.HourlyRecordId,
                        principalTable: "HourlyRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HourlyDowntimes_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DowntimeReasons_Code",
                table: "DowntimeReasons",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HourlyDowntimes_DowntimeReasonId",
                table: "HourlyDowntimes",
                column: "DowntimeReasonId");

            migrationBuilder.CreateIndex(
                name: "IX_HourlyDowntimes_HourlyRecordId_DowntimeReasonId",
                table: "HourlyDowntimes",
                columns: new[] { "HourlyRecordId", "DowntimeReasonId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HourlyDowntimes_UpdatedByUserId",
                table: "HourlyDowntimes",
                column: "UpdatedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HourlyDowntimes");

            migrationBuilder.DropTable(
                name: "DowntimeReasons");
        }
    }
}
