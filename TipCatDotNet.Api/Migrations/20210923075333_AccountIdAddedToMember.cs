using Microsoft.EntityFrameworkCore.Migrations;

namespace TipCatDotNet.Api.Migrations
{
    public partial class AccountIdAddedToMember : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "state",
                table: "members",
                newName: "State");

            migrationBuilder.RenameColumn(
                name: "permissions",
                table: "members",
                newName: "Permissions");

            migrationBuilder.RenameColumn(
                name: "modified",
                table: "members",
                newName: "Modified");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "members",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "created",
                table: "members",
                newName: "Created");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "members",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "qr_code_url",
                table: "members",
                newName: "QrCodeUrl");

            migrationBuilder.RenameColumn(
                name: "member_code",
                table: "members",
                newName: "MemberCode");

            migrationBuilder.RenameColumn(
                name: "last_name",
                table: "members",
                newName: "LastName");

            migrationBuilder.RenameColumn(
                name: "identity_hash",
                table: "members",
                newName: "IdentityHash");

            migrationBuilder.RenameColumn(
                name: "first_name",
                table: "members",
                newName: "FirstName");

            migrationBuilder.RenameColumn(
                name: "avatar_url",
                table: "members",
                newName: "AvatarUrl");

            migrationBuilder.AddColumn<int>(
                name: "AccountId",
                table: "members",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvitationCode",
                table: "members",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InvitationState",
                table: "members",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "accounts",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "commercial_name",
                table: "accounts",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256,
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "members");

            migrationBuilder.DropColumn(
                name: "InvitationCode",
                table: "members");

            migrationBuilder.DropColumn(
                name: "InvitationState",
                table: "members");

            migrationBuilder.RenameColumn(
                name: "State",
                table: "members",
                newName: "state");

            migrationBuilder.RenameColumn(
                name: "Permissions",
                table: "members",
                newName: "permissions");

            migrationBuilder.RenameColumn(
                name: "Modified",
                table: "members",
                newName: "modified");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "members",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "Created",
                table: "members",
                newName: "created");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "members",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "QrCodeUrl",
                table: "members",
                newName: "qr_code_url");

            migrationBuilder.RenameColumn(
                name: "MemberCode",
                table: "members",
                newName: "member_code");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "members",
                newName: "last_name");

            migrationBuilder.RenameColumn(
                name: "IdentityHash",
                table: "members",
                newName: "identity_hash");

            migrationBuilder.RenameColumn(
                name: "FirstName",
                table: "members",
                newName: "first_name");

            migrationBuilder.RenameColumn(
                name: "AvatarUrl",
                table: "members",
                newName: "avatar_url");

            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "accounts",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "commercial_name",
                table: "accounts",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256);
        }
    }
}
