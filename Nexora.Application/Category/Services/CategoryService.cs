using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Nexora.Application.Interfaces;
using Nexora.Application.Interfaces.Context;
using Nexora.Domain;

namespace Nexora.Application.Category.Services;

public class CategoryService:ICategoryService
{
    public IApplicationDbContext _context;

    public CategoryService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Domain.Entities.Category> AddCategory(CategoryCommand data)
    {
        if (data.Equals(null))
        {
            throw new BadHttpRequestException("There must be name of category");
        }

        Domain.Entities.Category newCategory = new Domain.Entities.Category(data.name);
        await _context.Categories.AddAsync(newCategory);
        await _context.SaveChangesAsync();
        return newCategory;
    }

    public async Task<Domain.Entities.Category> FindByName(string name)
    {
        if (name.IsNullOrEmpty())
        {
            throw new BadHttpRequestException("There must be name of category");

        }

        Domain.Entities.Category? category = await _context.Categories.FirstOrDefaultAsync(c=>c.Name==name);
        if (category == null)
        {
            throw new BadHttpRequestException("There is no category with name");
        }

        return category;
    }

    public async Task<List<Domain.Entities.Category>> GetAll()
    {
        var categories = await _context.Categories.ToListAsync();
        if (categories.IsNullOrEmpty())
        {
            throw new BadHttpRequestException("Failed to get categories");
        }

        return categories;
    }
}