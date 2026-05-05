using Microsoft.AspNetCore.Http;
using Nexora.Application.Review.Request;
using Nexora.Domain.DTOs;

namespace Nexora.Application.Interfaces.Repositories;

public interface IReviewRepository
{
    Task<bool> CreateAsync(Domain.Entities.Review review);
    Task<Domain.Entities.Review?> GetByIdAsync(Guid id);
    Task<IList<Domain.Entities.Review>> GetAllByListingIdAsync(Guid listingId);
    Task<bool> UpdateAsync(Domain.Entities.Review review);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}