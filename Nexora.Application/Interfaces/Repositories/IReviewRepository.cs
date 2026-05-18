

namespace Nexora.Application.Interfaces.Repositories;

public interface IReviewRepository
{
    Task<bool> CreateAsync(Review review);
    Task<Domain.Entities.Review?> GetByIdAsync(Guid? id);
    Task<IList<Review>> GetAllByListingIdAsync(Guid listingId);
    Task<bool> UpdateAsync(Review review);
    Task<bool> DeleteAsync(Guid? id);
    Task<bool> ExistsAsync(Guid id);
}