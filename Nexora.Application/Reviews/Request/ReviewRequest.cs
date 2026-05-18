namespace Nexora.Application.Reviews.Request;

public record ReviewRequest (Guid ListingId, int Rating, string Comment );