using Nexora.Domain.Common;

namespace Nexora.Domain.Entities;

public class FavoriteItem:BaseEntity
{
    public Guid ListingId { get; set; }
    public required Listing Listing { get; set; }
    public Guid FavoriteListId { get; set; }
    public FavoriteList FavoriteList { get; set; }
    
}