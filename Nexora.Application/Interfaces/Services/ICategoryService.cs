using Microsoft.AspNetCore.Http;
using Nexora.Application.Categories.Commands;

namespace Nexora.Application.Interfaces.Services;

public interface ICategoryService
{
    public Task<IResult> AddCategory(CategoryCommand data);

    public  Task<IResult> FindByName(string? name);

    public Task<IResult> GetAll();
}