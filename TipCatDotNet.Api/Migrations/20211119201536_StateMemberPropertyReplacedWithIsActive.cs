using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TipCatDotNet.Api.Migrations
{
    public partial class StateMemberPropertyReplacedWithIsActive : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "State",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "State",
                table: "Facilities");

            migrationBuilder.DropColumn(
                name: "State",
                table: "Accounts");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Members",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Facilities",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Accounts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("UPDATE public.\"Members\" SET \"IsActive\" = TRUE; " +
                "UPDATE public.\"Facilities\" SET \"IsActive\" = TRUE; UPDATE public.\"Accounts\" SET \"IsActive\" = TRUE");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Facilities");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Accounts");

            migrationBuilder.AddColumn<byte>(
                name: "State",
                table: "Members",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "State",
                table: "Facilities",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "State",
                table: "Accounts",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);
        }
    }
}
