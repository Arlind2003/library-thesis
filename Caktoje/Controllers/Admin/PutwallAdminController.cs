using Caktoje.Services.Admin;
using Microsoft.AspNetCore.Mvc;

namespace Caktoje.Controllers.Admin;

[ApiController]
[Route("api/admin/putwalls")]
public class PutwallAdminController : ControllerBase
{
    private readonly PutwallAdminService _putwallAdminService;

    public PutwallAdminController(PutwallAdminService putwallAdminService)
    {
        _putwallAdminService = putwallAdminService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPutwalls([FromQuery] string? query, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _putwallAdminService.GetPutwalls(query, page, pageSize);
        return Ok(result);
    }
}