using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TipCatDotNet.Api.Migrations
{
    public partial class FacilityDetails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CommercialName",
                table: "Facilities",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Facilities",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Facilities",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CommercialName",
                table: "Facilities");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Facilities");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Facilities");
        }
    }
}
