using System.ComponentModel.DataAnnotations;
using Nexora.Domain.Common;

namespace Nexora.Domain.Entities;

public class Category:BaseEntity
{
    public Category(string name)
    {
        Name = name;
    }

    [Required, MaxLength(10)] 
    public string Name { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
}