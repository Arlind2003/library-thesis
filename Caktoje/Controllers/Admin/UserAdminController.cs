using Caktoje.Data.Bindings;
using Caktoje.Services.Admin;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Caktoje.Controllers.Admin
{
    [Route("api/admin/users")]
    [ApiController]
    public class UserAdminController : ControllerBase
    {
        private readonly UserAdminService _userAdminService;

        public UserAdminController(UserAdminService userAdminService)
        {
            _userAdminService = userAdminService;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] string? query, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _userAdminService.GetUsers(query, page, pageSize);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] UserBinding model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _userAdminService.CreateUser(model);

            if (result.Succeeded)
            {
                return Ok();
            }

            return BadRequest(result.Errors);
        }
    }
}
