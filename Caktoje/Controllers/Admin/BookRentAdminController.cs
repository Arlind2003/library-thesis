using Caktoje.Data.Bindings;
using Caktoje.Services.Admin;
using Microsoft.AspNetCore.Mvc;

namespace Caktoje.Controllers.Admin;

[ApiController]
[Route("api/admin/book-rents")]
public class BookRentAdminController : ControllerBase
{
    private readonly BookRentAdminService _bookRentAdminService;

    public BookRentAdminController(BookRentAdminService bookRentAdminService)
    {
        _bookRentAdminService = bookRentAdminService;
    }

    [HttpGet]
    public async Task<IActionResult> GetBookRents([FromQuery] List<long>? bookStockIds, [FromQuery] List<string>? renterIds, [FromQuery] bool? overdue, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _bookRentAdminService.GetBookRents(bookStockIds, renterIds, overdue, page, pageSize);
        return Ok(result);
    }
    [HttpPost]
    public async Task<IActionResult> CreateBookRent([FromBody] BookRentBinding request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        var result = await _bookRentAdminService.CreateBookRent(request);
        return Ok(result);
        
    }

    [HttpPut("{id}/return")]
    public async Task<IActionResult> ReturnBook([FromRoute] long id)
    {
        await _bookRentAdminService.ReturnBook(id);
        return NoContent();
    }
}