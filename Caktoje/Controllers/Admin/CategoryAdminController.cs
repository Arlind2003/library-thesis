using Caktoje.Data.Bindings;
using Caktoje.Services.Admin;
using Microsoft.AspNetCore.Mvc;

namespace Caktoje.Controllers.Admin;

[ApiController]
[Route("api/admin/categories")]
public class CategoryAdminController : ControllerBase
{
    private readonly CategoryAdminService _categoryAdminService;

    public CategoryAdminController(CategoryAdminService categoryAdminService)
    {
        _categoryAdminService = categoryAdminService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCategories([FromQuery] string? query, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _categoryAdminService.GetCategories(query, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategory(long id)
    {
        var category = await _categoryAdminService.GetCategoryById(id);
        return category == null ? NotFound() : Ok(category);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromForm] CategoryBinding categoryBinding)
    {
        var category = await _categoryAdminService.CreateCategory(categoryBinding);
        return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(long id, [FromForm] CategoryBinding categoryBinding)
    {
        var category = await _categoryAdminService.UpdateCategory(id, categoryBinding);
        return category == null ? NotFound() : Ok(category);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(long id)
    {
        var result = await _categoryAdminService.DeleteCategory(id);
        return result ? NoContent() : NotFound();
    }
}