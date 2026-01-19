using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProdAnalysis.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDeviationEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeviationEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProductionDayId = table.Column<Guid>(type: "TEXT", nullable: false),
                    HourlyRecordId = table.Column<Guid>(type: "TEXT", nullable: false),
                    WorkCenterId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProductId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProductionDate = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    HourIndex = table.Column<int>(type: "INTEGER", nullable: false),
                    HourStart = table.Column<string>(type: "TEXT", maxLength: 5, nullable: false),
                    PlanQty = table.Column<int>(type: "INTEGER", nullable: false),
                    ActualQty = table.Column<int>(type: "INTEGER", nullable: false),
                    DeviationQty = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CurrentEscalationLevel = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AcknowledgedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AcknowledgedByUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ClosedByUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Note = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviationEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviationEvents_HourlyRecords_HourlyRecordId",
                        column: x => x.HourlyRecordId,
                        principalTable: "HourlyRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeviationEvents_ProductionDays_ProductionDayId",
                        column: x => x.ProductionDayId,
                        principalTable: "ProductionDays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeviationEvents_Users_AcknowledgedByUserId",
                        column: x => x.AcknowledgedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DeviationEvents_Users_ClosedByUserId",
                        column: x => x.ClosedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DeviationEvents_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EscalationLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DeviationEventId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Level = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Message = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EscalationLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EscalationLogs_DeviationEvents_DeviationEventId",
                        column: x => x.DeviationEventId,
                        principalTable: "DeviationEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeviationEvents_AcknowledgedByUserId",
                table: "DeviationEvents",
                column: "AcknowledgedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviationEvents_ClosedByUserId",
                table: "DeviationEvents",
                column: "ClosedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviationEvents_CreatedByUserId",
                table: "DeviationEvents",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviationEvents_HourlyRecordId",
                table: "DeviationEvents",
                column: "HourlyRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviationEvents_ProductionDate_WorkCenterId",
                table: "DeviationEvents",
                columns: new[] { "ProductionDate", "WorkCenterId" });

            migrationBuilder.CreateIndex(
                name: "IX_DeviationEvents_ProductionDayId",
                table: "DeviationEvents",
                column: "ProductionDayId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviationEvents_Status_CreatedAt",
                table: "DeviationEvents",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_EscalationLogs_DeviationEventId_Level_CreatedAt",
                table: "EscalationLogs",
                columns: new[] { "DeviationEventId", "Level", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EscalationLogs");

            migrationBuilder.DropTable(
                name: "DeviationEvents");
        }
    }
}
