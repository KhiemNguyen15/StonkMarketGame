using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StonkMarketGame.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddShortCodeToPendingTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ShortCode",
                table: "PendingTransactions",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            // Backfill existing pending transactions with sequential short codes
            migrationBuilder.Sql(@"
                WITH numbered_rows AS (
                    SELECT ""Id"", ROW_NUMBER() OVER (ORDER BY ""RequestedAt"") AS row_num
                    FROM ""PendingTransactions""
                )
                UPDATE ""PendingTransactions""
                SET ""ShortCode"" = numbered_rows.row_num
                FROM numbered_rows
                WHERE ""PendingTransactions"".""Id"" = numbered_rows.""Id"";
            ");

            // Reset the identity sequence to continue from the max value
            migrationBuilder.Sql(@"
                SELECT setval(
                    pg_get_serial_sequence('""PendingTransactions""', 'ShortCode'),
                    COALESCE((SELECT MAX(""ShortCode"") FROM ""PendingTransactions""), 0) + 1,
                    false
                );
            ");

            migrationBuilder.CreateIndex(
                name: "IX_PendingTransactions_ShortCode",
                table: "PendingTransactions",
                column: "ShortCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PendingTransactions_ShortCode",
                table: "PendingTransactions");

            migrationBuilder.DropColumn(
                name: "ShortCode",
                table: "PendingTransactions");
        }
    }
}
