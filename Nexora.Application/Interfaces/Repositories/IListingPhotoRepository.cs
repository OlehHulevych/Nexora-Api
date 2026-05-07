namespace Nexora.Application.Interfaces.Repositories;

public interface IListingPhotoRepository
{
    public Task<bool> Add(IList<ProductImage> images);
    public Task<bool> Update(ProductImage image);
    public Task<ProductImage?> GetById(Guid id);
    public Task<IList<ProductImage>> GetByListingId(Guid id);
    public Task<ProductImage> GetByPath(string path);
    public Task<bool> Delete(Guid id);
    public Task<bool> DeleteRange(IList<ProductImage> images);

}