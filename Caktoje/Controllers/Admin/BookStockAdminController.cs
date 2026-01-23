using Caktoje.Data.Bindings;
using Caktoje.Services.Admin;
using Microsoft.AspNetCore.Mvc;

namespace Caktoje.Controllers.Admin;

[ApiController]
[Route("api/admin/book-stocks")]
public class BookStockAdminController : ControllerBase
{
    private readonly BookStockAdminService _bookStockAdminService;

    public BookStockAdminController(BookStockAdminService bookStockAdminService)
    {
        _bookStockAdminService = bookStockAdminService;
    }

    [HttpGet]
    public async Task<IActionResult> GetBookStocks([FromQuery] List<long>? bookIds, [FromQuery] List<long>? putwallSectionIds, [FromQuery] string? query, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _bookStockAdminService.GetBookStocks(bookIds, putwallSectionIds, query, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetBookStock(long id)
    {
        var bookStock = await _bookStockAdminService.GetBookStockById(id);
        return bookStock == null ? NotFound() : Ok(bookStock);
    }

    [HttpPost]
    public async Task<IActionResult> CreateBookStock([FromBody] BookStockBinding bookStockBinding)
    {
        var bookStock = await _bookStockAdminService.CreateBookStock(bookStockBinding);
        return CreatedAtAction(nameof(GetBookStock), new { id = bookStock.Id }, bookStock);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBookStock(long id, [FromBody] BookStockBinding bookStockBinding)
    {
        var bookStock = await _bookStockAdminService.UpdateBookStock(id, bookStockBinding);
        return bookStock == null ? NotFound() : Ok(bookStock);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBookStock(long id)
    {
        var result = await _bookStockAdminService.DeleteBookStock(id);
        return result ? NoContent() : NotFound();
    }
}