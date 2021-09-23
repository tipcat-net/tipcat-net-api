using Microsoft.EntityFrameworkCore.Migrations;

namespace TipCatDotNet.Api.Migrations
{
    public partial class TablesRenamed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_members",
                table: "members");

            migrationBuilder.DropPrimaryKey(
                name: "PK_facilities",
                table: "facilities");

            migrationBuilder.DropPrimaryKey(
                name: "PK_accounts",
                table: "accounts");

            migrationBuilder.RenameTable(
                name: "members",
                newName: "Members");

            migrationBuilder.RenameTable(
                name: "facilities",
                newName: "Facilities");

            migrationBuilder.RenameTable(
                name: "accounts",
                newName: "Accounts");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Facilities",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Facilities",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "state",
                table: "Accounts",
                newName: "State");

            migrationBuilder.RenameColumn(
                name: "phone",
                table: "Accounts",
                newName: "Phone");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Accounts",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "modified",
                table: "Accounts",
                newName: "Modified");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "Accounts",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "created",
                table: "Accounts",
                newName: "Created");

            migrationBuilder.RenameColumn(
                name: "address",
                table: "Accounts",
                newName: "Address");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Accounts",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "commercial_name",
                table: "Accounts",
                newName: "CommercialName");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Members",
                table: "Members",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Facilities",
                table: "Facilities",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Accounts",
                table: "Accounts",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Members",
                table: "Members");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Facilities",
                table: "Facilities");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Accounts",
                table: "Accounts");

            migrationBuilder.RenameTable(
                name: "Members",
                newName: "members");

            migrationBuilder.RenameTable(
                name: "Facilities",
                newName: "facilities");

            migrationBuilder.RenameTable(
                name: "Accounts",
                newName: "accounts");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "facilities",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "facilities",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "State",
                table: "accounts",
                newName: "state");

            migrationBuilder.RenameColumn(
                name: "Phone",
                table: "accounts",
                newName: "phone");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "accounts",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Modified",
                table: "accounts",
                newName: "modified");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "accounts",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "Created",
                table: "accounts",
                newName: "created");

            migrationBuilder.RenameColumn(
                name: "Address",
                table: "accounts",
                newName: "address");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "accounts",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "CommercialName",
                table: "accounts",
                newName: "commercial_name");

            migrationBuilder.AddPrimaryKey(
                name: "PK_members",
                table: "members",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_facilities",
                table: "facilities",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_accounts",
                table: "accounts",
                column: "id");
        }
    }
}
