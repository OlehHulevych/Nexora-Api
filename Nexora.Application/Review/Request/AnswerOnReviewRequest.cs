namespace Nexora.Application.Review.Request;

public record AnswerOnReviewRequest(Guid? ReviewId, Guid? ListingId, string Comment);