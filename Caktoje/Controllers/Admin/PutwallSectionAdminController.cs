using Caktoje.Data.Bindings;
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

    [HttpPost]
    public async Task<IActionResult> CreatePutwallSection([FromBody] PutwallSectionBinding binding)
    {
        var result = await _putwallSectionAdminService.CreatePutwallSection(binding);
        return CreatedAtAction(nameof(GetPutwallSections), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePutwallSection(long id, [FromBody] PutwallSectionBinding binding)
    {
        var result = await _putwallSectionAdminService.UpdatePutwallSection(id, binding);
        if (result == null)
        {
            return NotFound();
        }
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePutwallSection(long id)
    {
        var result = await _putwallSectionAdminService.DeletePutwallSection(id);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }
}