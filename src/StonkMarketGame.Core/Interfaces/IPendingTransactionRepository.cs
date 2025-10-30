using StonkMarketGame.Core.Entities;

namespace StonkMarketGame.Core.Interfaces;

/// <summary>
/// Repository for managing pending transactions.
/// </summary>
public interface IPendingTransactionRepository
{
    /// <summary>
    /// Adds a new pending transaction.
    /// </summary>
    Task AddAsync(PendingTransaction transaction, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all pending transactions scheduled for execution.
    /// </summary>
    Task<IReadOnlyList<PendingTransaction>> GetPendingAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all pending transactions for a specific user.
    /// </summary>
    Task<IReadOnlyList<PendingTransaction>> GetUserPendingOrdersAsync(ulong userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a pending transaction by ID.
    /// </summary>
    Task<PendingTransaction?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing pending transaction.
    /// </summary>
    Task UpdateAsync(PendingTransaction transaction, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a pending transaction.
    /// </summary>
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
}
