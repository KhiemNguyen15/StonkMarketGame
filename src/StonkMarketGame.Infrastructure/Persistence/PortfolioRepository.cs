using Microsoft.EntityFrameworkCore;
using StonkMarketGame.Core.Entities;
using StonkMarketGame.Core.Interfaces;

namespace StonkMarketGame.Infrastructure.Persistence;

public class PortfolioRepository : IPortfolioRepository
{
    private readonly AppDbContext _db;

    public PortfolioRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<UserPortfolio> GetOrCreatePortfolioAsync(ulong userId)
    {
        var portfolio = await _db.Portfolios
            .Include(p => p.Holdings)
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (portfolio == null)
        {
            portfolio = new UserPortfolio(userId);
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
}