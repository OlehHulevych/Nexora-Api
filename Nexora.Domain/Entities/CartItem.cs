using Nexora.Domain.Common;

namespace Nexora.Domain.Entities;

public class CartItem:BaseEntity
{
    public Guid CartId { get; set; }
    public Cart Cart { get; set; }
    public Guid ListingId { get; set; }
    public Listing Listing { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}