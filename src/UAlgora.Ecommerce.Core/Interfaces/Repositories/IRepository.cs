using System.Linq.Expressions;
using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Core.Interfaces.Repositories;

/// <summary>
/// Base repository interface for common CRUD operations.
/// </summary>
/// <typeparam name="T">Entity type.</typeparam>
public interface IRepository<T> where T : BaseEntity
{
    /// <summary>
    /// Gets an entity by ID.
    /// </summary>
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets all entities.
    /// </summary>
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Finds entities matching a predicate.
    /// </summary>
    Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);

    /// <summary>
    /// Gets the first entity matching a predicate.
    /// </summary>
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);

    /// <summary>
    /// Checks if any entity matches the predicate.
    /// </summary>
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);

    /// <summary>
    /// Counts entities matching a predicate.
    /// </summary>
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default);

    /// <summary>
    /// Adds a new entity.
    /// </summary>
    Task<T> AddAsync(T entity, CancellationToken ct = default);

    /// <summary>
    /// Adds multiple entities.
    /// </summary>
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    Task<T> UpdateAsync(T entity, CancellationToken ct = default);

    /// <summary>
    /// Updates multiple entities.
    /// </summary>
    Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);

    /// <summary>
    /// Deletes an entity by ID.
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Deletes an entity.
    /// </summary>
    Task DeleteAsync(T entity, CancellationToken ct = default);

    /// <summary>
    /// Deletes multiple entities.
    /// </summary>
    Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);
}

/// <summary>
/// Base repository interface for entities with soft delete support.
/// </summary>
/// <typeparam name="T">Entity type.</typeparam>
public interface ISoftDeleteRepository<T> : IRepository<T> where T : SoftDeleteEntity
{
    /// <summary>
    /// Soft deletes an entity by ID.
    /// </summary>
    Task SoftDeleteAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Restores a soft-deleted entity.
    /// </summary>
    Task RestoreAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets all entities including soft-deleted ones.
    /// </summary>
    Task<IReadOnlyList<T>> GetAllWithDeletedAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets only soft-deleted entities.
    /// </summary>
    Task<IReadOnlyList<T>> GetDeletedAsync(CancellationToken ct = default);
}
