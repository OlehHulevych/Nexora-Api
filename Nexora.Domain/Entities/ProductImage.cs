using Nexora.Domain.Common;

namespace Nexora.Domain.Entities;

public class ProductImage:BaseEntity
{
    public ProductImage(Guid productId, string url, int sortOrder)
    {
        ProductId = productId;
        Url = url;
        SortOrder = sortOrder;
    }

    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public string Url { get; set; }
    public int SortOrder { get; set; }
    
}