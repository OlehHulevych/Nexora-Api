using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Nexora.Application.Interfaces.Context;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Domain.Entities;
using Nexora.Domain.Exceptions;

namespace Nexora.Infrastructure.Repository;

public class CategoryRepository : ICategoryRepository
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CategoryRepository> _logger;

    public CategoryRepository(IApplicationDbContext context, ILogger<CategoryRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Guid> AddCategory(Category category)
    {
        _logger.LogInformation("Adding category {CategoryName}", category.Name);
        try
        {
            var response = await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            if (response.Equals(null))
            {
                _logger.LogError("Failed to create category {CategoryName}", category.Name);
                throw new BadHttpRequestException("Failed to create new category");
            }

            _logger.LogInformation("Category {CategoryName} added with Id {CategoryId}",
                category.Name, response.Entity.Id);
            return response.Entity.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add category {CategoryName}", category.Name);
            throw;
        }
    }

    public async Task<Category?> GetCategory(string name)
    {
        _logger.LogInformation("Fetching category by name {CategoryName}", name);

        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Name == name);
        if (category == null)
        {
            _logger.LogWarning("Category {CategoryName} not found", name);
            return null;
        }

        _logger.LogInformation("Category {CategoryName} fetched successfully", name);
        return category;
    }

    public async Task<List<Category>?> GetCategories()
    {
        _logger.LogInformation("Fetching all categories");
        try
        {
            var categories = await _context.Categories.ToListAsync();
            _logger.LogInformation("Fetched {Count} categories", categories.Count);
            return categories;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch categories");
            throw;
        }
    }
}