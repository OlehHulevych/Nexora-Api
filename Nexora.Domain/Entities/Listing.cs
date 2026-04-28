using System.ComponentModel.DataAnnotations.Schema;
using Nexora.Domain.Common;

namespace Nexora.Domain.Entities;

public class Listing:BaseEntity
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int? StockQuantity { get; set; }
    public bool isActive { get; set; } = true;
    public string SellerId { get; set; }
    public ApplicationUser? Seller { get; set; }
    public Guid CategoryId { get; set; }
    public Category? Category { get; set; }
    
    public List<CartItem>? CartItems { get; set; }
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();

    public void Update(string? name, string? description, decimal? price, int? stockQuantity)
    {
        if (name is not null) Name = name;
        if (description is not null) Description = description;
        if (price is not null) Price = price.Value;
        if (stockQuantity is not null) StockQuantity = stockQuantity.Value;

        UpdatedAt = DateTime.UtcNow;
    }
}