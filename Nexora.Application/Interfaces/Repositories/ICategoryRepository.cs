using Microsoft.AspNetCore.Http;
using Nexora.Application.Category;

namespace Nexora.Application.Interfaces.Repositories;

public interface ICategoryRepository
{
    public Task<IResult> AddCategory(CategoryCommand data);

    public Task<IResult> GetCategory(string? name);
    
        
}