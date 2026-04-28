namespace Nexora.Application.Review.Request;

public record ReviewRequest (string UserId, Guid ListingId, int Rating, string Comment );