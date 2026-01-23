using Caktoje.Services.Admin;
using Microsoft.AspNetCore.Mvc;

namespace Caktoje.Controllers.Admin;

[ApiController]
[Route("api/admin/days-closed")]
public class DayClosedAdminController : ControllerBase
{
    private readonly DayClosedAdminService _dayClosedAdminService;

    public DayClosedAdminController(DayClosedAdminService dayClosedAdminService)
    {
        _dayClosedAdminService = dayClosedAdminService;
    }

    [HttpGet]
    public async Task<IActionResult> GetDaysClosed([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _dayClosedAdminService.GetDaysClosed(page, pageSize);
        return Ok(result);
    }
}