using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Models.Domain;
using UAlgora.Ecommerce.Infrastructure.Data;

namespace UAlgora.Ecommerce.Infrastructure.Repositories;

/// <summary>
/// Generic repository implementation for basic CRUD operations.
/// </summary>
/// <typeparam name="T">Entity type.</typeparam>
public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly EcommerceDbContext Context;
    protected readonly DbSet<T> DbSet;

    public Repository(EcommerceDbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await DbSet.FindAsync(new object[] { id }, ct);
    }

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default)
    {
        return await DbSet.ToListAsync(ct);
    }

    public virtual async Task<IReadOnlyList<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken ct = default)
    {
        return await DbSet.Where(predicate).ToListAsync(ct);
    }

    public virtual async Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken ct = default)
    {
        return await DbSet.FirstOrDefaultAsync(predicate, ct);
    }

    public virtual async Task<bool> AnyAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken ct = default)
    {
        return await DbSet.AnyAsync(predicate, ct);
    }

    public virtual async Task<int> CountAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken ct = default)
    {
        if (predicate == null)
            return await DbSet.CountAsync(ct);
        return await DbSet.CountAsync(predicate, ct);
    }

    public virtual async Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        await DbSet.AddAsync(entity, ct);
        await Context.SaveChangesAsync(ct);
        return entity;
    }

    public virtual async Task AddRangeAsync(
        IEnumerable<T> entities,
        CancellationToken ct = default)
    {
        var entityList = entities.ToList();
        await DbSet.AddRangeAsync(entityList, ct);
        await Context.SaveChangesAsync(ct);
    }

    public virtual async Task<T> UpdateAsync(T entity, CancellationToken ct = default)
    {
        DbSet.Update(entity);
        await Context.SaveChangesAsync(ct);
        return entity;
    }

    public virtual async Task UpdateRangeAsync(
        IEnumerable<T> entities,
        CancellationToken ct = default)
    {
        DbSet.UpdateRange(entities);
        await Context.SaveChangesAsync(ct);
    }

    public virtual async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await GetByIdAsync(id, ct);
        if (entity != null)
        {
            DbSet.Remove(entity);
            await Context.SaveChangesAsync(ct);
        }
    }

    public virtual async Task DeleteAsync(T entity, CancellationToken ct = default)
    {
        DbSet.Remove(entity);
        await Context.SaveChangesAsync(ct);
    }

    public virtual async Task DeleteRangeAsync(
        IEnumerable<T> entities,
        CancellationToken ct = default)
    {
        DbSet.RemoveRange(entities);
        await Context.SaveChangesAsync(ct);
    }
}

/// <summary>
/// Repository implementation for entities that support soft delete.
/// </summary>
/// <typeparam name="T">Entity type that extends SoftDeleteEntity.</typeparam>
public class SoftDeleteRepository<T> : Repository<T>, ISoftDeleteRepository<T> where T : SoftDeleteEntity
{
    public SoftDeleteRepository(EcommerceDbContext context) : base(context)
    {
    }

    public virtual async Task SoftDeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await GetByIdAsync(id, ct);
        if (entity != null)
        {
            entity.IsDeleted = true;
            entity.DeletedAt = DateTime.UtcNow;
            await Context.SaveChangesAsync(ct);
        }
    }

    public virtual async Task RestoreAsync(Guid id, CancellationToken ct = default)
    {
        // Need to bypass the global filter to find deleted entities
        var entity = await DbSet
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.Id == id, ct);

        if (entity != null)
        {
            entity.IsDeleted = false;
            entity.DeletedAt = null;
            await Context.SaveChangesAsync(ct);
        }
    }

    public virtual async Task<IReadOnlyList<T>> GetAllWithDeletedAsync(CancellationToken ct = default)
    {
        return await DbSet
            .IgnoreQueryFilters()
            .ToListAsync(ct);
    }

    public virtual async Task<IReadOnlyList<T>> GetDeletedAsync(CancellationToken ct = default)
    {
        return await DbSet
            .IgnoreQueryFilters()
            .Where(e => e.IsDeleted)
            .ToListAsync(ct);
    }
}
