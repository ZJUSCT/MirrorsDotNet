using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Orchestrator.Migrations
{
    /// <inheritdoc />
    public partial class AddLastSuccessAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastSuccessAt",
                table: "SavedInfos",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastSuccessAt",
                table: "SavedInfos");
        }
    }
}
