namespace DeliveryApp.Domain.Entities;

public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public string? ImageUrl { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public Guid CategoryId { get; private set; }

    public Category Category { get; private set; } = null!;
    public ICollection<OrderItem> OrderItems { get; private set; } = [];

    private Product() { }

    public static Product Create(string name, string description, decimal price, string? imageUrl, Guid categoryId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name cannot be empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Product description cannot be empty.", nameof(description));

        if (price <= 0)
            throw new ArgumentException("Product price must be greater than zero.", nameof(price));

        if (categoryId == Guid.Empty)
            throw new ArgumentException("Category ID cannot be empty.", nameof(categoryId));

        return new Product
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Price = price,
            ImageUrl = imageUrl,
            IsActive = true,
            CategoryId = categoryId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string description, decimal price, string? imageUrl, Guid categoryId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name cannot be empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Product description cannot be empty.", nameof(description));

        if (price <= 0)
            throw new ArgumentException("Product price must be greater than zero.", nameof(price));

        if (categoryId == Guid.Empty)
            throw new ArgumentException("Category ID cannot be empty.", nameof(categoryId));

        Name = name;
        Description = description;
        Price = price;
        ImageUrl = imageUrl;
        CategoryId = categoryId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        if (IsActive)
            throw new InvalidOperationException("Product is already active.");

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new InvalidOperationException("Product is already inactive.");

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
