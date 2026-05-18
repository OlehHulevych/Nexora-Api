using Nexora.Domain.DTOs;
using Nexora.Domain.Entities;

namespace Nexora.Domain.Mappers;

public static class ReviewLikeMapper
{
    public static ReviewLikeDto ToDto(ReviewLike reviewLike)
    {
        return new ReviewLikeDto(reviewLike.Author.FirstName + " "+reviewLike.Author.LastName,reviewLike.Act.ToString());
    }
}