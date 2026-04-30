using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Nexora.Application.Category;
using Nexora.Application.Category.Services;
using Nexora.Application.Interfaces.Repositories;

namespace Nexora.Infrastructure.Repository;

public class CategoryRepository:ICategoryRepository
{
    private readonly CategoryService _categoryService;

    public CategoryRepository(CategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    public async Task<IResult> AddCategory(CategoryCommand data)
    {
        var response = await _categoryService.AddCategory(data);
        if (response.Equals(null))
        {
            return Results.BadRequest("Failed to Create new category");
        }

        return Results.Ok(response);
    }

    public async Task<IResult> GetCategory(string? name)
    {
        
        if (name != null)
        {
            var category = await _categoryService.FindByName(name);
            if (category.Name.IsNullOrEmpty())
            {
                return Results.BadRequest("Failed to get category");
            }

            return Results.Ok(new {message="Getting category was successfully", category = category});
        }

        var categories = await _categoryService.GetAll();
        if (categories.IsNullOrEmpty())
        {
            return Results.BadRequest("Failed to get categories");
        }

        return Results.Ok(new { message = "Categories are fetched", categories = categories });
        
    }
    
}