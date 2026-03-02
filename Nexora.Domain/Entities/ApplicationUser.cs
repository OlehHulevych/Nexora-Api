using System.ComponentModel.DataAnnotations;
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
    public ICollection<Product> ProductAsSeller { get; set; } = new List<Product>();
    public Avatar Avatar { get; set; }
    public ICollection<Order> OrderAsBuyer { get; set; } = new List<Order>();
    public Address? Address { get; set; }
    public ICollection<Review> ReviewWritten { get; set; } = new List<Review>();
}