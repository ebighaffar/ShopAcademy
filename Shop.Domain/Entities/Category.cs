using Shop.Domain.Constants;
using Shop.Domain.Exceptions;

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

    public Category(string name, string? description = null)
    {
        EnsureValid(name, description);

        Name = name.Trim();
        Description = description;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void Update(string name, string? description)
    {
        EnsureValid(name, description);

        Name = name.Trim();
        Description = description;
    }

    private static void EnsureValid(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > CategoryConstants.NameMaxLength)
            throw new DomainValidationException(
                $"Category name is required and cannot exceed {CategoryConstants.NameMaxLength} characters");

        if (description is { Length: > CategoryConstants.DescriptionMaxLength })
            throw new DomainValidationException(
                $"Category description cannot exceed {CategoryConstants.DescriptionMaxLength} characters");
    }
}
