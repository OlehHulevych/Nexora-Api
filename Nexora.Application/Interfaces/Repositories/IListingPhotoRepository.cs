namespace Nexora.Application.Interfaces.Repositories;

public interface IListingPhotoRepository
{
    public Task<bool> Add(ProductImage image);
    public Task<bool> Update(ProductImage image);
    public Task<ProductImage?> GetById(Guid id);
    public Task<IList<ProductImage>> GetByListingId(Guid id);
    public Task<bool> Delete(Guid id);
}