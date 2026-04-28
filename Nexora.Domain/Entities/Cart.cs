using Nexora.Domain.Common;

namespace Nexora.Domain.Entities;

public class Cart:BaseEntity
{
    public string UserId { get; set; }
    public ApplicationUser User { get; set;}
    public List<CartItem> items { get; set; } = new();
}