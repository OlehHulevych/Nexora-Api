using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
namespace Nexora.Domain.Entities;



public class ApplicationUser:IdentityUser
{

    [Required]
    [MaxLength(20)]
    public string? FirstName { get; set; }
    [Required]
    [MaxLength(20)]
    public string? LastName { get; set; }
    [JsonIgnore]
    public ICollection<Listing> ProductAsSeller { get; set; } = new List<Listing>();
    public Avatar? Avatar { get; set; }
    public ICollection<Order> OrderAsBuyer { get; set; } = new List<Order>();
    public Address? Address { get; set; }
    public Cart? Cart { get; set; }
    public ICollection<Review> ReviewWritten { get; set; } = new List<Review>();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool Banned { get; set; } = false;
}