using Microsoft.EntityFrameworkCore;
using StonkMarketGame.Core.Entities;
using StonkMarketGame.Core.ValueObjects;

namespace StonkMarketGame.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<UserPortfolio> Portfolios { get; set; }
    public DbSet<Transaction> Transactions { get; set; }

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

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Ticker)
                .HasConversion(
                    v => v.Value,
                    v => new TickerSymbol(v));
            entity.Property(t => t.Type)
                .HasConversion<string>();
        });
    }
}