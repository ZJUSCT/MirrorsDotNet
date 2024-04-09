using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Orchestrator.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SavedInfos",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 42, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    LastSyncAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Size = table.Column<ulong>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavedInfos", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SavedInfos");
        }
    }
}
