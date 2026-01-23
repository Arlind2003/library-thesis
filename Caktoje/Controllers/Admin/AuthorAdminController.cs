using Caktoje.Data.Bindings;
using Caktoje.Services.Admin;
using Microsoft.AspNetCore.Mvc;

namespace Caktoje.Controllers.Admin;

[ApiController]
[Route("api/admin/authors")]
public class AuthorAdminController : ControllerBase
{
    private readonly AuthorAdminService _authorAdminService;

    public AuthorAdminController(AuthorAdminService authorAdminService)
    {
        _authorAdminService = authorAdminService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAuthors([FromQuery] string? query, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _authorAdminService.GetAuthors(query, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAuthor(long id)
    {
        var author = await _authorAdminService.GetAuthorById(id);
        return author == null ? NotFound() : Ok(author);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAuthor([FromForm] AuthorBinding authorBinding)
    {
        var author = await _authorAdminService.CreateAuthor(authorBinding);
        return CreatedAtAction(nameof(GetAuthor), new { id = author.Id }, author);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAuthor(long id, [FromForm] AuthorBinding authorBinding)
    {
        var author = await _authorAdminService.UpdateAuthor(id, authorBinding);
        return author == null ? NotFound() : Ok(author);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAuthor(long id)
    {
        var result = await _authorAdminService.DeleteAuthor(id);
        return result ? NoContent() : NotFound();
    }
}