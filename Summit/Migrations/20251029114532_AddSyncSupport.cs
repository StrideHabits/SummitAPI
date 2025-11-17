using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SummitAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddSyncSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "Habits",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Habits",
                type: "BLOB",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "HabitCompletions",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "HabitCompletions",
                type: "BLOB",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.CreateTable(
                name: "RequestLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RequestId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    ResultJson = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HabitCompletions_UpdatedAt",
                table: "HabitCompletions",
                column: "UpdatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_RequestLogs_UserId_RequestId",
                table: "RequestLogs",
                columns: new[] { "UserId", "RequestId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RequestLogs");

            migrationBuilder.DropIndex(
                name: "IX_HabitCompletions_UpdatedAt",
                table: "HabitCompletions");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "Habits");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Habits");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "HabitCompletions");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "HabitCompletions");
        }
    }
}
