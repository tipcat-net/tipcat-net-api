using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TipCatDotNet.Api.Migrations
{
    public partial class TransactionAmountIndexAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transactions_Created",
                table: "Transactions");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_Amount",
                table: "Transactions",
                column: "Amount")
                .Annotation("Npgsql:IndexSortOrder", new[] { SortOrder.Ascending, SortOrder.Descending });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_Created",
                table: "Transactions",
                column: "Created")
                .Annotation("Npgsql:IndexSortOrder", new[] { SortOrder.Ascending, SortOrder.Descending });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transactions_Amount",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_Created",
                table: "Transactions");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_Created",
                table: "Transactions",
                column: "Created")
                .Annotation("Npgsql:IndexSortOrder", new[] { SortOrder.Descending });
        }
    }
}
