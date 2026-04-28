using Nexora.Domain.Common;

namespace Nexora.Domain.Entities;

public class Review:BaseEntity
{
    

    public Guid ProductId { get; set; }
    public Listing? Product { get; set; }
    public string AuthorId { get; set; }
    public ApplicationUser? Author { get; set; } = default!;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    
}