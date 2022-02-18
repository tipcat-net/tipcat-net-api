using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TipCatDotNet.Api.Migrations
{
    public partial class AccountCurrencyWasAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Accounts",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Accounts");
        }
    }
}
