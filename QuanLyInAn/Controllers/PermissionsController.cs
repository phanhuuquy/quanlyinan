using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyInAn.Services;
using QuanLyInAn.DTOs;
[Authorize(Roles = "1")]
[ApiController]
[Route("api/[controller]")]
public class PermissionsController : ControllerBase
{
    private readonly PermissionsService _permissionsService;

    public PermissionsController(PermissionsService permissionsService)
    {
        _permissionsService = permissionsService;
    }

    [HttpPost("assign-role")]
    public async Task<IActionResult> AssignRoleToUser([FromBody] AssignRoleDto dto)
    {
        var result = await _permissionsService.AssignRoleToUserAsync(dto.EmployeeId, dto.RoleId);

        if (result.Success)
            return Ok(new { Message = result.Message });
        else
            return BadRequest(new { Message = result.Message });
    }
}
