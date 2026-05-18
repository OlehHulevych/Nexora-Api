namespace Nexora.Application.Reviews.Request;

public record AnswerOnReviewRequest(Guid? ReviewId, Guid? ListingId, string Comment);