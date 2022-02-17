using Microsoft.EntityFrameworkCore.Migrations;
using TipCatDotNet.Api.Data.Models.HospitalityFacility;

#nullable disable

namespace TipCatDotNet.Api.Migrations
{
    public partial class ApplicationPreferences : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationPreferences",
                table: "Members",
                type: "jsonb",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.AddColumn<AccountPreferences>(
                name: "Preferences",
                table: "Accounts",
                type: "jsonb",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApplicationPreferences",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "Preferences",
                table: "Accounts");
        }
    }
}
