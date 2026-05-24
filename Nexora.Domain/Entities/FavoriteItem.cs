using Nexora.Domain.Common;

namespace Nexora.Domain.Entities;

public class FavoriteItem:BaseEntity
{
    public Guid ListingId { get; set; }
    public Listing Listing { get; set; } = null!;
    public Guid FavoriteListId { get; set; }
    public FavoriteList FavoriteList { get; set; } = null!;

}