using GEST.Application.Abstractions.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GEST.Infrastructure.Persistence.Repositories;

public class BaseRepository<T>(
    GestDbContext db
    ) : IBaseRepository<T> where T : class
{
    protected readonly GestDbContext _db = db;
    protected readonly DbSet<T> _set = db.Set<T>();

    public virtual async Task<T?> GetByIdAsync(object id, CancellationToken ct = default)
        => await _set.FindAsync(new[] { id }, ct);

    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default)
        => await _set.AsNoTracking().ToListAsync(ct);

    public virtual async Task<IEnumerable<T>> GetAllAsync(string includes, CancellationToken ct = default)
    {
        IQueryable<T> query = _set.AsNoTracking();

        if (!string.IsNullOrEmpty(includes))
        {
            foreach (string include in includes.Split([','], StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(include);
            }
        }

        return await query.ToListAsync(ct);
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        => await _set.AsNoTracking().Where(predicate).ToListAsync(ct);

    public virtual async Task AddAsync(T entity, CancellationToken ct = default)
        => await _set.AddAsync(entity, ct);

    public virtual async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
        => await _set.AddRangeAsync(entities, ct);

    public virtual void Update(T entity)
        => _set.Update(entity);

    public virtual void Remove(T entity)
        => _set.Remove(entity);

    public virtual void RemoveRange(IEnumerable<T> entities)
        => _set.RemoveRange(entities);
}
