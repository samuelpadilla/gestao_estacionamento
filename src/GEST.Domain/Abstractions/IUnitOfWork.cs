using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace GEST.Domain.Abstractions;

public interface IUnitOfWork : IAsyncDisposable
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);

    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default);

    Task BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken ct = default);

    Task CommitAsync(CancellationToken ct = default);

    Task RollbackAsync(CancellationToken ct = default);

    /// <summary>
    /// Executa uma função dentro de transação (abre/commita/rollback automaticamente).
    /// Útil para Handlers/Services que precisam de atomicidade.
    /// </summary>
    Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> action,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        int concurrencyRetryCount = 3,
        CancellationToken ct = default);

    /// <summary>
    /// Versão void.
    /// </summary>
    Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> action,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        int concurrencyRetryCount = 3,
        CancellationToken ct = default);
}
