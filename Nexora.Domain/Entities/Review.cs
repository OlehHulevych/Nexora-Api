using System.ComponentModel.DataAnnotations;
using Nexora.Domain.Common;

namespace Nexora.Domain.Entities;

public class Review:BaseEntity
{
    public Guid ProductId { get; set; }
    public Listing? Product { get; set; }
    
    public Guid? MainReviewId { get; set; }
    public Review? MainReview { get; set; }
    [Required]
    
    [MaxLength(128)]
    public required string AuthorId { get; set; }
    public ApplicationUser? Author { get; set; } 
    public int? Rating { get; set; }
    
    [MaxLength(500)]
    public string? Comment { get; set; }
    public int? Likes { get; set; }
    public List<Review> Reviews { get; set; } = new List<Review>();

}