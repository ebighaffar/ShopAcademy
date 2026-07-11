namespace Shop.Domain.Entities;

public class Category
{
    public long Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private Category()
    {
    }

    public Category(long id, string name)
    {
        Id = id;
        Name = name;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void Update(string name, string? description)
    {
        Name = name;
        Description = description;
    }
}
