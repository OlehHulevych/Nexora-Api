using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Nexora.Application.Category;
using Nexora.Application.Category.Services;
using Nexora.Application.Interfaces.Context;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Domain.Entities;
using Nexora.Domain.Exceptions;

namespace Nexora.Infrastructure.Repository;

public class CategoryRepository:ICategoryRepository
{

    private readonly IApplicationDbContext _context;
    
    public CategoryRepository(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> AddCategory(Category category)
    {
        var response = await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();
        if (response.Equals(null))
        {
            throw new BadHttpRequestException("Failed to create new category");
        }

        return response.Entity.Id;
    }
    

    public async Task<Category?> GetCategory(string name)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c=>c.Name==name);
        if (category != null && category.Name.IsNullOrEmpty())
        {
            throw new NotFoundException(nameof(Category), name);
        }

        return category;      
    }

    public async Task<List<Category>?> GetCategories()
    {
        return await _context.Categories.ToListAsync();
    }
    
}