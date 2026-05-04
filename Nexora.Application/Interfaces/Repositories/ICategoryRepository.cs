

namespace Nexora.Application.Interfaces.Repositories;

public interface ICategoryRepository
{
    public Task<Guid> AddCategory(Domain.Entities.Category category);

    public Task<Domain.Entities.Category?> GetCategory(string name);
    public Task<List<Domain.Entities.Category>?> GetCategories();


}