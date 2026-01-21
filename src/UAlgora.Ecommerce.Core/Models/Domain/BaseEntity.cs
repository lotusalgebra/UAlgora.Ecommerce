namespace UAlgora.Ecommerce.Core.Models.Domain;

/// <summary>
/// Base entity with common properties for all domain models.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Base entity with soft delete support.
/// </summary>
public abstract class SoftDeleteEntity : BaseEntity
{
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
