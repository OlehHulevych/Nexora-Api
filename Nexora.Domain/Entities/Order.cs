using System.ComponentModel.DataAnnotations.Schema;
using Nexora.Domain.Common;

namespace Nexora.Domain.Entities;

public enum OrderStatus
{
    Pending =0,
    Paid = 1,
    Delivered = 3,
    Canceled = 4,
}

public class Order:BaseEntity
{

    public string BuyerId { get; set; }
    public ApplicationUser? Buyer { get; set; }
    public OrderStatus Status { get; set; }
    [Column(TypeName = "decimal(10,2)")]
    public decimal TotalAmount { get; set; }
    public Guid DeliveredAddressId { get; set; }
    public Address? DeliveredAddress { get; set; }
    public List<OrderItem>? Items { get; set; } = new List<OrderItem>();

}

public class OrderItem:BaseEntity
{

    public Guid OrderId { get; set; }
    public Order? Order { get; set; }
    public Guid ProductId { get; set; }
    public Listing? Product { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}