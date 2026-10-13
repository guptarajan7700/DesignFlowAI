namespace DesignFlow.Domain.Entities;

public abstract class AuditableEntity
{
    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Guid? CreatedBy { get; set; }
}
