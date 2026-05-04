using Microsoft.AspNetCore.Http;
using Nexora.Application.Category;

namespace Nexora.Application.Interfaces;

public interface ICategoryService
{
    public Task<IResult> AddCategory(CategoryCommand data);

    public  Task<IResult> FindByName(string? name);

    public Task<IResult> GetAll();
}