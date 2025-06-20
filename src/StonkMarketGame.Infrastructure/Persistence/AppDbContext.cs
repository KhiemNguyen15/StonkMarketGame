using Microsoft.EntityFrameworkCore;
using StonkMarketGame.Core.Entities;
using StonkMarketGame.Core.ValueObjects;

namespace StonkMarketGame.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public DbSet<UserPortfolio> Portfolios { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserPortfolio>(entity =>
        {
            entity.HasKey(p => p.UserId);

            entity.OwnsMany(p => p.Holdings, holdings =>
            {
                holdings.WithOwner().HasForeignKey("PortfolioId");
                holdings.Property(h => h.Ticker)
                    .HasConversion(
                        v => v.Value,
                        v => new TickerSymbol(v));
                holdings.HasKey("PortfolioId", "Ticker");
            });
        });
    }
}