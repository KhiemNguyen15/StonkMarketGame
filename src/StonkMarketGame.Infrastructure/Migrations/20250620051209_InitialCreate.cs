using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StonkMarketGame.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Portfolios",
                columns: table => new
                {
                    UserId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    CashBalance = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Portfolios", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "StockHolding",
                columns: table => new
                {
                    Ticker = table.Column<string>(type: "text", nullable: false),
                    PortfolioId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    AveragePrice = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockHolding", x => new { x.PortfolioId, x.Ticker });
                    table.ForeignKey(
                        name: "FK_StockHolding_Portfolios_PortfolioId",
                        column: x => x.PortfolioId,
                        principalTable: "Portfolios",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StockHolding");

            migrationBuilder.DropTable(
                name: "Portfolios");
        }
    }
}
