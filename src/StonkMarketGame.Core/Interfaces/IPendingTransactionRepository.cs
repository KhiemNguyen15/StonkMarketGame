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
    /// <summary>
/// Retrieve a pending transaction by its identifier.
/// </summary>
/// <param name="id">The identifier of the pending transaction to retrieve.</param>
/// <returns>The pending transaction with the specified identifier, or null if not found.</returns>
    Task<PendingTransaction?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a pending transaction by short code.
    /// <summary>
/// Retrieves a pending transaction that matches the specified short code.
/// </summary>
/// <param name="shortCode">Numeric short code assigned to the pending transaction.</param>
/// <returns>The matching <see cref="PendingTransaction"/> if found, otherwise <c>null</c>.</returns>
    Task<PendingTransaction?> GetByShortCodeAsync(int shortCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing pending transaction.
    /// </summary>
    Task UpdateAsync(PendingTransaction transaction, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a pending transaction.
    /// </summary>
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
}