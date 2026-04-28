using System.ComponentModel.DataAnnotations;
using Nexora.Domain.Common;

namespace Nexora.Domain.Entities;

public class Address:BaseEntity
{
    public string UserId { get; set; }
    public ApplicationUser? User { get; set; }
    [Required, MaxLength(120)]
    public string Line1 { get; set; } = default!;
    [MaxLength(120)]
    public string? Line2 { get; set; }

    public string City { get; set; } = default!;
    [Required, MaxLength(80)]
    public string Country { get; set; }
    [MaxLength(20)]
    public string? PostalCode { get; set; }

    public Address(String userId, string line1, string city, string country, string? postalCode,
        string? line2)
    {
        UserId = userId;
        Line1 = line1;
        Line2 = line2;
        City = city;
        Country = country;
        PostalCode = postalCode;
    }
}