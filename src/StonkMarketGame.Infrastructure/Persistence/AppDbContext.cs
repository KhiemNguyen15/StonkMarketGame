using Microsoft.EntityFrameworkCore;
using StonkMarketGame.Core.Entities;
using StonkMarketGame.Core.ValueObjects;

namespace StonkMarketGame.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<UserPortfolio> Portfolios { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<PendingTransaction> PendingTransactions { get; set; }

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

        modelBuilder.Entity<PendingTransaction>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.ShortCode)
                .ValueGeneratedOnAdd()
                .UseIdentityColumn();
            entity.Property(t => t.Ticker)
                .HasConversion(
                    v => v.Value,
                    v => new TickerSymbol(v));
            entity.Property(t => t.Type)
                .HasConversion<string>();
            entity.Property(t => t.Status)
                .HasConversion<string>();
            entity.HasIndex(t => new { t.Status, t.ScheduledFor });
            entity.HasIndex(t => t.UserId);
            entity.HasIndex(t => t.ShortCode).IsUnique();
        });
    }
}