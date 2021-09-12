using Microsoft.EntityFrameworkCore.Migrations;

namespace TipCatDotNet.Api.Migrations
{
    public partial class AddTemporaryColumnsToMemberTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "email_tmp",
                table: "members",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "first_name_tmp",
                table: "members",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "last_name_tmp",
                table: "members",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "verification_code_hash",
                table: "members",
                type: "character varying(6)",
                maxLength: 6,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "email_tmp",
                table: "members");

            migrationBuilder.DropColumn(
                name: "first_name_tmp",
                table: "members");

            migrationBuilder.DropColumn(
                name: "last_name_tmp",
                table: "members");

            migrationBuilder.DropColumn(
                name: "verification_code_hash",
                table: "members");
        }
    }
}
