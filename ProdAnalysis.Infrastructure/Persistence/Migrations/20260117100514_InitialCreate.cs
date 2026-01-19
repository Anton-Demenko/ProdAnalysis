using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProdAnalysis.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Role = table.Column<int>(type: "INTEGER", nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkCenters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkCenters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductionDays",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Date = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    ShiftStart = table.Column<string>(type: "TEXT", maxLength: 5, nullable: false),
                    ShiftEnd = table.Column<string>(type: "TEXT", maxLength: 5, nullable: false),
                    WorkCenterId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProductId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TaktSec = table.Column<int>(type: "INTEGER", nullable: false),
                    PlanPerHour = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionDays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionDays_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionDays_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionDays_WorkCenters_WorkCenterId",
                        column: x => x.WorkCenterId,
                        principalTable: "WorkCenters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HourlyRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProductionDayId = table.Column<Guid>(type: "TEXT", nullable: false),
                    HourIndex = table.Column<int>(type: "INTEGER", nullable: false),
                    HourStart = table.Column<string>(type: "TEXT", maxLength: 5, nullable: false),
                    PlanQty = table.Column<int>(type: "INTEGER", nullable: false),
                    ActualQty = table.Column<int>(type: "INTEGER", nullable: true),
                    Comment = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HourlyRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HourlyRecords_ProductionDays_ProductionDayId",
                        column: x => x.ProductionDayId,
                        principalTable: "ProductionDays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HourlyRecords_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HourlyRecords_ProductionDayId_HourIndex",
                table: "HourlyRecords",
                columns: new[] { "ProductionDayId", "HourIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HourlyRecords_UpdatedByUserId",
                table: "HourlyRecords",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionDays_CreatedByUserId",
                table: "ProductionDays",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionDays_Date_ShiftStart_WorkCenterId",
                table: "ProductionDays",
                columns: new[] { "Date", "ShiftStart", "WorkCenterId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionDays_ProductId",
                table: "ProductionDays",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionDays_WorkCenterId",
                table: "ProductionDays",
                column: "WorkCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Code",
                table: "Products",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkCenters_Name",
                table: "WorkCenters",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HourlyRecords");

            migrationBuilder.DropTable(
                name: "ProductionDays");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "WorkCenters");
        }
    }
}
