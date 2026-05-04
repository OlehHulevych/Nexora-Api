using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexora.Application.Category;
using Nexora.Application.Interfaces;
using Nexora.Domain.Constants;

namespace Nexora.Api.Controllers;
[ApiController]
[Route("api/category")]
public class CategoryController : Controller
{
    private readonly ICategoryService _categoryService;
    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }
    
    [Authorize(Roles = RoleNames.Admin)]
    [HttpPost]
    public async Task<IResult> AddCategory([FromForm] CategoryCommand data)
    {
        return await _categoryService.AddCategory(data);
    }
    [HttpGet]
    public async Task<IResult> GetCategory ([FromQuery] string? name )
    {
        return await _categoryService.FindByName(name);
    }
    [HttpGet]
    public async Task<IResult> GetCategories ()
    {
        return await _categoryService.GetAll();
    }
}