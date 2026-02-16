using Caktoje.Constants.Enums;
using Caktoje.Data.Bindings;
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

    [HttpPost]
    public async Task<IActionResult> CreateDayClosed([FromBody] DayClosedBinding binding){
        if(!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        var result = await _dayClosedAdminService.CreateDayClosed(binding);
        return Ok(result);
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDayClosed(long id, [FromBody] DayClosedBinding binding)
    {
        if(!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        var result = await _dayClosedAdminService.UpdateDayClosed(id, binding);
        return result == null ? NotFound() : Ok(result);
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDayClosed(long id)
    {
        await _dayClosedAdminService.DeleteDayClosed(id);
        return NoContent();
    }
}