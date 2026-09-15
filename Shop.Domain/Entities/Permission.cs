namespace Shop.Domain.Entities;

public class Permission
{
    public long Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public bool IsSystem { get; private set; }

    private Permission()
    {
    }

    public Permission(string name, string? description = null, bool isSystem = false)
    {
        Name = name.Trim().ToLowerInvariant();
        Description = description;
        IsSystem = isSystem;
    }

    public void MarkAsSystem() => IsSystem = true;
}
