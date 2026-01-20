using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProdAnalysis.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixPendingModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductionDays_Date_ShiftStart_WorkCenterId",
                table: "ProductionDays");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionDays_Date_ShiftStart_WorkCenterId_ProductId",
                table: "ProductionDays",
                columns: new[] { "Date", "ShiftStart", "WorkCenterId", "ProductId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductionDays_Date_ShiftStart_WorkCenterId_ProductId",
                table: "ProductionDays");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionDays_Date_ShiftStart_WorkCenterId",
                table: "ProductionDays",
                columns: new[] { "Date", "ShiftStart", "WorkCenterId" },
                unique: true);
        }
    }
}
