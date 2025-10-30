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

    /// <summary>
    /// Retrieve a pending transaction by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the pending transaction to retrieve.</param>
    /// <returns>The matching <see cref="PendingTransaction"/> instance, or null if no match is found.</returns>
    public async Task<PendingTransaction?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _db.PendingTransactions
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    /// <summary>
    /// Retrieves the pending transaction that has the specified short code.
    /// </summary>
    /// <param name="shortCode">The short code assigned to the pending transaction.</param>
    /// <returns>The matching PendingTransaction, or null if none exists.</returns>
    public async Task<PendingTransaction?> GetByShortCodeAsync(int shortCode, CancellationToken cancellationToken = default)
    {
        return await _db.PendingTransactions
            .FirstOrDefaultAsync(t => t.ShortCode == shortCode, cancellationToken);
    }

    /// <summary>
    /// Persists updates to an existing PendingTransaction in the database.
    /// </summary>
    /// <param name="transaction">The PendingTransaction entity with updated values to save.</param>
    /// <param name="cancellationToken">Token to cancel the save operation.</param>
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