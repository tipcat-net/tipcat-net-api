using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TipCatDotNet.Api.Migrations
{
    public partial class TransactionAmountToDecimalAndCreatedIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "Transactions",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_Created",
                table: "Transactions",
                column: "Created")
                .Annotation("Npgsql:IndexSortOrder", new[] { SortOrder.Descending });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transactions_Created",
                table: "Transactions");

            migrationBuilder.AlterColumn<long>(
                name: "Amount",
                table: "Transactions",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");
        }
    }
}
