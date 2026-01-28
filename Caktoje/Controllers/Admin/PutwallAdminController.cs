using Caktoje.Data.Bindings;
using Caktoje.Exceptions;
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
    [HttpPost]
    public async Task<IActionResult> CreatePutwall([FromBody] PutwallBinding createDto)
    {
        var result = await _putwallAdminService.CreatePutwall(createDto) ?? throw new BadRequestException("Failed to create putwall.");
        return CreatedAtAction(nameof(GetPutwalls), new { id = result.Id }, result);
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePutwall(int id, [FromBody] PutwallBinding updateDto)
    {
        var result = await _putwallAdminService.UpdatePutwall(id, updateDto);
        if (result == null)
        {
            return NotFound();
        }
        return Ok(result);
    }
}