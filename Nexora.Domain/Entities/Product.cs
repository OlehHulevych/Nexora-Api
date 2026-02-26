using System.ComponentModel.DataAnnotations.Schema;
using Nexora.Domain.Common;

namespace Nexora.Domain.Entities;

public class Product:BaseEntity
{
    public Product(string name, string? description, decimal price, int? stockQuantity, bool isActive, string sellerId,  Guid categoryId)
    {
        Name = name;
        Description = description;
        Price = price;
        StockQuantity = stockQuantity;
        this.isActive = isActive;
        SellerId = sellerId;
        CategoryId = categoryId;
    }

    public string Name { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int? StockQuantity { get; set; }
    public bool isActive { get; set; }
    public string SellerId { get; set; }
    public ApplicationUser? Seller { get; set; }
    public Guid CategoryId { get; set; }
    public Category? Category { get; set; }
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}