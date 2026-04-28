using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Nexora.Domain.Common;

namespace Nexora.Domain.Entities;

public class Category:BaseEntity
{
    public Category(string name)
    {
        Name = name;
    }

    [Required, MaxLength(20)] 
    public string Name { get; set; }
    [JsonIgnore]
    public ICollection<Listing> Products { get; set; } = new List<Listing>();
}