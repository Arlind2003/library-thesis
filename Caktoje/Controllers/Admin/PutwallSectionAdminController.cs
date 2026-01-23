using Caktoje.Services.Admin;
using Microsoft.AspNetCore.Mvc;

namespace Caktoje.Controllers.Admin;

[ApiController]
[Route("api/admin/putwall-sections")]
public class PutwallSectionAdminController : ControllerBase
{
    private readonly PutwallSectionAdminService _putwallSectionAdminService;

    public PutwallSectionAdminController(PutwallSectionAdminService putwallSectionAdminService)
    {
        _putwallSectionAdminService = putwallSectionAdminService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPutwallSections([FromQuery] string? query, [FromQuery] List<long>? putwallIds, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _putwallSectionAdminService.GetPutwallSections(query, putwallIds, page, pageSize);
        return Ok(result);
    }
}