


namespace Nexora.Application.Interfaces.Repositories;

public interface ICategoryRepository
{
    public Task<Guid> AddCategory(Category category);

    public Task<Category?> GetCategory(string name);
    public Task<List<Category>?> GetCategories();


}