using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TipCatDotNet.Api.Migrations
{
    public partial class PaymentClientMessagePropertyAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "Transactions",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Message",
                table: "Transactions");
        }
    }
}
