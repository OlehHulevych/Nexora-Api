using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexora.Application.Categories.Commands;
using Nexora.Application.Interfaces.Services;
using Nexora.Domain.Constants;

namespace Nexora.Api.Controllers;
[ApiController]
[Route("api/category")]
public class CategoryController : Controller
{
    private readonly ICategoryService _categoryService;
    private readonly ILogger<CategoryController> _logger;
    
    public CategoryController(ICategoryService categoryService, ILogger<CategoryController> logger)
    {
        _categoryService = categoryService;
        _logger = logger;
    }
    
    /// <summary>
    /// Creating new category
    /// </summary>
    ///<param name="data">contain name for creating new category</param>
    /// <returns>Created category</returns>
    /// <response code="200">Category added successfully</response>
    /// <response code="404">Failed to create new category</response>
    [Authorize(Roles = RoleNames.Admin)]
    [Route("/add")]
    [HttpPost]
    public async Task<IResult> AddCategory([FromForm] CategoryCommand data)
    {
        _logger.LogInformation("Category Controller - adding new category");
        return await _categoryService.AddCategory(data);
    }
    /// <summary>
    /// Getting category by their name and its listings
    /// </summary>
    /// <param name="name">name of category</param>
    /// <returns>list of listings according to category name</returns>
    /// <response code="200">Listing were fetched</response>
    /// <response code="404">Category is not exist or failed</response>
    [HttpGet]
    [Route("/get/one")]
    public async Task<IResult> GetCategory ([FromQuery] string? name )
    {
        _logger.LogInformation("Category Controller - getting one category and its products");
        return await _categoryService.FindByName(name);
    }
    /// <summary>
    /// Getting all categories
    /// </summary>
    /// <returns>all categories</returns>
    /// <response code="200">Categories were fetched successfully</response>
    /// <response code="404">Failed to get categories</response>
    [HttpGet]
    [Route("/get/all")]
    public async Task<IResult> GetCategories ()
    {
        _logger.LogInformation("Category controller - getting all controllers");
        return await _categoryService.GetAll();
    }
}