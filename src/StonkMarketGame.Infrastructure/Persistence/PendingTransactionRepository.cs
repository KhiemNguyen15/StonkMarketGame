using Microsoft.EntityFrameworkCore;
using StonkMarketGame.Core.Entities;
using StonkMarketGame.Core.Interfaces;

namespace StonkMarketGame.Infrastructure.Persistence;

public class PendingTransactionRepository : IPendingTransactionRepository
{
    private readonly AppDbContext _db;

    public PendingTransactionRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(PendingTransaction transaction, CancellationToken cancellationToken = default)
    {
        _db.PendingTransactions.Add(transaction);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PendingTransaction>> GetPendingAsync(CancellationToken cancellationToken = default)
    {
        return await _db.PendingTransactions
            .Where(t => t.Status == PendingTransactionStatus.Pending)
            .OrderBy(t => t.ScheduledFor)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PendingTransaction>> GetUserPendingOrdersAsync(
        ulong userId,
        CancellationToken cancellationToken = default)
    {
        return await _db.PendingTransactions
            .Where(t => t.UserId == userId && t.Status == PendingTransactionStatus.Pending)
            .OrderBy(t => t.ScheduledFor)
            .ToListAsync(cancellationToken);
    }

    public async Task<PendingTransaction?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _db.PendingTransactions
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task UpdateAsync(PendingTransaction transaction, CancellationToken cancellationToken = default)
    {
        _db.PendingTransactions.Update(transaction);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var transaction = await GetByIdAsync(id, cancellationToken);
        if (transaction != null)
        {
            _db.PendingTransactions.Remove(transaction);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
