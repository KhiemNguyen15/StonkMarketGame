using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StonkMarketGame.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFailureReasonToPendingTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FailureReason",
                table: "PendingTransactions",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FailureReason",
                table: "PendingTransactions");
        }
    }
}
