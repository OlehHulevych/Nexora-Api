using Nexora.Domain.Common;

namespace Nexora.Domain.Entities;

public class Review:BaseEntity
{
    public Review(Guid productId, string authorId,  int rating, string? comment)
    {
        ProductId = productId;
        AuthorId = authorId;
        Rating = rating;
        Comment = comment;
    }

    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public string AuthorId { get; set; }
    public ApplicationUser? Author { get; set; } = default!;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    
}