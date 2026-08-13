using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeamSchedule.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTimeSlotsToActivities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ActivityCandidateDates_ActivityId_CandidateDate",
                table: "ActivityCandidateDates");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "FinalEndTime",
                table: "TeamActivities",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "FinalStartTime",
                table: "TeamActivities",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "EndTime",
                table: "ActivityCandidateDates",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "StartTime",
                table: "ActivityCandidateDates",
                type: "time",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ActivityCandidateDates_ActivityId_CandidateDate_StartTime_EndTime",
                table: "ActivityCandidateDates",
                columns: new[] { "ActivityId", "CandidateDate", "StartTime", "EndTime" },
                unique: true,
                filter: "[StartTime] IS NOT NULL AND [EndTime] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ActivityCandidateDates_ActivityId_CandidateDate_StartTime_EndTime",
                table: "ActivityCandidateDates");

            migrationBuilder.DropColumn(
                name: "FinalEndTime",
                table: "TeamActivities");

            migrationBuilder.DropColumn(
                name: "FinalStartTime",
                table: "TeamActivities");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "ActivityCandidateDates");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "ActivityCandidateDates");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityCandidateDates_ActivityId_CandidateDate",
                table: "ActivityCandidateDates",
                columns: new[] { "ActivityId", "CandidateDate" },
                unique: true);
        }
    }
}
