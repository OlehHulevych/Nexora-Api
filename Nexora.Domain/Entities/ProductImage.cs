using System.Text.Json.Serialization;
using Nexora.Domain.Common;

namespace Nexora.Domain.Entities;

public class ProductImage:BaseEntity
{
    public ProductImage(Guid productId, string url, string filePath )
    {
        ProductId = productId;
        Url = url;
        FilePath = filePath;
    }

    public Guid ProductId { get; set; }
    [JsonIgnore]
    public Listing? Product { get; set; }
    public string FilePath { get; set; }
    public string Url { get; set; }
    
    
}