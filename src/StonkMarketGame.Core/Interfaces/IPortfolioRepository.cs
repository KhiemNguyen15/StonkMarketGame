using StonkMarketGame.Core.Entities;

namespace StonkMarketGame.Core.Interfaces;

public interface IPortfolioRepository
{
    Task<UserPortfolio> GetOrCreatePortfolioAsync(ulong userId);
    Task SaveAsync(UserPortfolio portfolio);
}