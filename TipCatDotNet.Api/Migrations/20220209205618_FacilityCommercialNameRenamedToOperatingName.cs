using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TipCatDotNet.Api.Migrations
{
    public partial class FacilityCommercialNameRenamedToOperatingName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CommercialName",
                table: "Facilities",
                newName: "OperatingName");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OperatingName",
                table: "Facilities",
                newName: "CommercialName");
        }
    }
}
