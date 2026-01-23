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
}