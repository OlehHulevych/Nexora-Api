namespace Nexora.Application.Review.Request;

public record AnswerOnReviewRequest(Guid? ReviewId, string? UserId, Guid ListingId, string Comment);