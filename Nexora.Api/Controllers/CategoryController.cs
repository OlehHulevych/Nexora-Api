using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexora.Application.Category;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Domain.Constants;
using Nexora.Infrastructure.Repository;

namespace Nexora.Api.Controllers;
[ApiController]
[Route("api/category")]
public class CategoryController : Controller
{
    private readonly ICategoryRepository _categoryRepository;
    public CategoryController(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }
    
    [Authorize(Roles = RoleNames.Admin)]
    [HttpPost]
    public async Task<IResult> AddCategory([FromForm] CategoryCommand data)
    {
        return await _categoryRepository.AddCategory(data);
    }
    [HttpGet]
    public async Task<IResult> GetCategory ([FromQuery] string? name )
    {
        return await _categoryRepository.GetCategory(name);
    }
}