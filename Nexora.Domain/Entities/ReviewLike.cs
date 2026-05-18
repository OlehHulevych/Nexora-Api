using Nexora.Domain.Common;
using Nexora.Domain.Enums;

namespace Nexora.Domain.Entities;

public class ReviewLike:BaseEntity
{
    public Guid ReviewId { get; set; }
    public required Review Review { get; set; }
    public string AuthorId { get; set; }
    public required ApplicationUser Author { get; set; }
    public ReviewActs Act { get; set; }
    
}