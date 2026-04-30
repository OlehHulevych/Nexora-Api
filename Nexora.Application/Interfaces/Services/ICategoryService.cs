using Nexora.Application.Category;

namespace Nexora.Application.Interfaces;

public interface ICategoryService
{
    public Task<Domain.Entities.Category> AddCategory(CategoryCommand data);

    public  Task<Domain.Entities.Category> FindByName(string name);

    public Task<List<Domain.Entities.Category>> GetAll();
}