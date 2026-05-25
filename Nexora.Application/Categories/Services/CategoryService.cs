using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Nexora.Application.Categories.Commands;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Application.Interfaces.Services;

namespace Nexora.Application.Categories.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(ICategoryRepository categoryRepository, ILogger<CategoryService> logger)
    {
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    public async Task<IResult> AddCategory(CategoryCommand data)
    {
        _logger.LogInformation("Adding new category {CategoryName}", data.name);

        if (data.Equals(null))
        {
            _logger.LogWarning("Add category failed — no data provided");
            throw new BadHttpRequestException("There must be name of category");
        }

        Category newCategory = new Category(data.name);
        await _categoryRepository.AddCategory(newCategory);

        _logger.LogInformation("Category {CategoryName} added successfully", data.name);
        return Results.Ok(new { message = "The category was added", Category = newCategory });
    }

    public async Task<IResult> FindByName(string? name)
    {
        _logger.LogInformation("Fetching category by name {CategoryName}", name);

        if (name.IsNullOrEmpty() || name == null)
        {
            _logger.LogWarning("Find category failed — name is empty");
            throw new BadHttpRequestException("There must be name of category");
        }

        Category? category = await _categoryRepository.GetCategory(name);
        if (category == null)
        {
            _logger.LogWarning("Find category failed — category {CategoryName} not found", name);
            throw new BadHttpRequestException("There is no category with name");
        }

        _logger.LogInformation("Category {CategoryName} fetched successfully", name);
        return Results.Ok(new { message = "The category was retrieved", category });
    }

    public async Task<IResult> GetAll()
    {
        _logger.LogInformation("Fetching all categories");

        var categories = await _categoryRepository.GetCategories();
        if (categories.IsNullOrEmpty())
        {
            _logger.LogWarning("Get all categories failed — no categories found");
            throw new BadHttpRequestException("Failed to get categories");
        }

        _logger.LogInformation("Fetched {Count} categories", categories.Count());
        return Results.Ok(new { message = "The categories are retrieved", categories });
    }
}