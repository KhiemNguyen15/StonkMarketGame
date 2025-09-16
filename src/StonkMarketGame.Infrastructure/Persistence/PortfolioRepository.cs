using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StonkMarketGame.Core.Configuration;
using StonkMarketGame.Core.Entities;
using StonkMarketGame.Core.Interfaces;

namespace StonkMarketGame.Infrastructure.Persistence;

public class PortfolioRepository : IPortfolioRepository
{
    private readonly AppDbContext _db;
    private readonly GameSettings _gameSettings;

    public PortfolioRepository(AppDbContext db, IOptions<GameSettings> gameSettings)
    {
        _db = db;
        _gameSettings = gameSettings.Value;
    }

    public async Task<UserPortfolio> GetOrCreatePortfolioAsync(ulong userId)
    {
        var portfolio = await _db.Portfolios
            .Include(p => p.Holdings)
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (portfolio == null)
        {
            portfolio = new UserPortfolio(userId, _gameSettings.DefaultStartingBalance);
            _db.Portfolios.Add(portfolio);
            await _db.SaveChangesAsync();
        }

        return portfolio;
    }

    public async Task SaveAsync(UserPortfolio portfolio)
    {
        _db.Portfolios.Update(portfolio);
        await _db.SaveChangesAsync();
    }

    public async Task SaveTransactionAsync(Transaction transaction)
    {
        _db.Transactions.Add(transaction);
        await _db.SaveChangesAsync();
    }

    public async Task SavePortfolioAndTransactionAsync(UserPortfolio portfolio, Transaction transaction)
    {
        _db.Portfolios.Update(portfolio);
        _db.Transactions.Add(transaction);
        await _db.SaveChangesAsync();
    }

    public async Task<List<Transaction>> GetTransactionHistoryAsync(ulong userId, int limit = 50)
    {
        return await _db.Transactions
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.Timestamp)
            .Take(limit)
            .ToListAsync();
    }
}