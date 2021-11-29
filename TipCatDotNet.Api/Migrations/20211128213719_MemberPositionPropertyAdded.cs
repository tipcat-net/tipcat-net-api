using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TipCatDotNet.Api.Migrations
{
    public partial class MemberPositionPropertyAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Position",
                table: "Members",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Position",
                table: "Members");
        }
    }
}
