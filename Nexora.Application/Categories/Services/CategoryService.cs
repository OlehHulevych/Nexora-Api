using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Nexora.Application.Categories.Commands;
using Nexora.Application.Interfaces;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Application.Interfaces.Services;

namespace Nexora.Application.Categories.Services;

public class CategoryService:ICategoryService
{
    public readonly ICategoryRepository CategoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        CategoryRepository = categoryRepository;
    }

    public async Task<IResult> AddCategory(CategoryCommand data)
    {
        if (data.Equals(null))
        {
            throw new BadHttpRequestException("There must be name of category");
        }
        Category newCategory = new Category(data.name);
        await CategoryRepository.AddCategory(newCategory);
        return Results.Ok(new {message = "The category was added", Category = newCategory});
    }

    public async Task<IResult> FindByName(string? name)
    {
        if (name.IsNullOrEmpty()|| name == null)
        {
            throw new BadHttpRequestException("There must be name of category");

        }

        Category? category = await CategoryRepository.GetCategory(name);
        if (category == null)
        {
            throw new BadHttpRequestException("There is no category with name");
        }

        return Results.Ok(new {message = "The category was retrieved", category});
    }

    public async Task<IResult> GetAll()
    {
        var categories = await CategoryRepository.GetCategories();
        if (categories.IsNullOrEmpty())
        {
            throw new BadHttpRequestException("Failed to get categories");
        }

        return Results.Ok(new {message = "The categories are retrieved", categories});
    }
}