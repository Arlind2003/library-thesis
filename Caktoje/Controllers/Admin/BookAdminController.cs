using Caktoje.Data.Bindings;
using Caktoje.Services.Admin;
using Microsoft.AspNetCore.Mvc;

namespace Caktoje.Controllers.Admin;

[ApiController]
[Route("api/admin/books")]
public class BookAdminController : ControllerBase
{
    private readonly BookAdminService _bookAdminService;

    public BookAdminController(BookAdminService bookAdminService)
    {
        _bookAdminService = bookAdminService;
    }

    [HttpGet]
    public async Task<IActionResult> GetBooks([FromQuery] List<long>? categoryIds, [FromQuery] string? query, [FromQuery] List<long>? authorIds, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _bookAdminService.GetBooks(categoryIds, query, authorIds, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetBook(long id)
    {
        var book = await _bookAdminService.GetBookById(id);
        return book == null ? NotFound() : Ok(book);
    }

    [HttpPost]
    public async Task<IActionResult> CreateBook([FromForm] BookBinding bookBinding)
    {
        var book = await _bookAdminService.CreateBook(bookBinding);
        return CreatedAtAction(nameof(GetBook), new { id = book.Id }, book);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBook(long id, [FromForm] BookBinding bookBinding)
    {
        var book = await _bookAdminService.UpdateBook(id, bookBinding);
        return book == null ? NotFound() : Ok(book);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBook(long id)
    {
        var result = await _bookAdminService.DeleteBook(id);
        return result ? NoContent() : NotFound();
    }
}