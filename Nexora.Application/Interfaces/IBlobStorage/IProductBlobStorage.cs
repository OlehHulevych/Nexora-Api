using Microsoft.AspNetCore.Http;
using Nexora.Domain.Entities;

namespace Nexora.Application.Interfaces.IBlobStorage;

public interface IProductBlobStorage
{
    Task<IReadOnlyList<ProductImage>> UploadAsync(List<IFormFile> files, string folder, Guid id, Listing listing,  CancellationToken ct = default);
    Task<bool> DeleteAsync(ICollection<ProductImage> photoList);
    Task<bool> DeleteForEditing(ICollection<string> photoList);

}