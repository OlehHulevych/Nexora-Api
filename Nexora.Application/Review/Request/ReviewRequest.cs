namespace Nexora.Application.Review.Request;

public record ReviewRequest (Guid ListingId, int Rating, string Comment );