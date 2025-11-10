using GEST.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace GEST.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly GestDbContext _db;
    private IDbContextTransaction? _currentTx;

    public UnitOfWork(GestDbContext db) => _db = db;

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default)
        => await _db.Database.BeginTransactionAsync(ct);

    public async Task BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken ct = default)
    {
        if (_currentTx is not null)
            return; // já em transação (idempotência simples)

        _currentTx = await _db.Database.BeginTransactionAsync(isolationLevel, ct);
    }

    public async Task CommitAsync(CancellationToken ct = default)
    {
        if (_currentTx is null) return;

        await _db.SaveChangesAsync(ct);
        await _currentTx.CommitAsync(ct);
        await _currentTx.DisposeAsync();
        _currentTx = null;
    }

    public async Task RollbackAsync(CancellationToken ct = default)
    {
        if (_currentTx is null) return;

        await _currentTx.RollbackAsync(ct);
        await _currentTx.DisposeAsync();
        _currentTx = null;
    }

    public async Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> action,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        int concurrencyRetryCount = 3,
        CancellationToken ct = default)
    {
        int attempt = 0;

        while (true)
        {
            ct.ThrowIfCancellationRequested();
            await BeginTransactionAsync(isolationLevel, ct);

            try
            {
                var result = await action(ct);
                await CommitAsync(ct);
                return result;
            }
            catch (DbUpdateConcurrencyException) when (attempt++ < concurrencyRetryCount)
            {
                await RollbackAsync(ct);
                // EF Core refaz o snapshot otimista; pequenos delays ajudam em cenários ruidosos
                await Task.Delay(TimeSpan.FromMilliseconds(100 * attempt), ct);
            }
            catch
            {
                await RollbackAsync(ct);
                throw;
            }
        }
    }

    public Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> action,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        int concurrencyRetryCount = 3,
        CancellationToken ct = default)
        => ExecuteInTransactionAsync<object?>(async c =>
        {
            await action(c);
            return null;
        }, isolationLevel, concurrencyRetryCount, ct);

    public async ValueTask DisposeAsync()
    {
        if (_currentTx is not null)
        {
            await _currentTx.DisposeAsync();
            _currentTx = null;
        }
    }
}
