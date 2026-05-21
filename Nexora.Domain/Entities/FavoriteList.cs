using Nexora.Domain.Common;

namespace Nexora.Domain.Entities;

public class FavoriteList:BaseEntity
{
    public required string UserId { get; set; }
    public required ApplicationUser User { get; set; }
    public IList<FavoriteItem> FavoriteItems { get; set; } = new List<FavoriteItem>();
}