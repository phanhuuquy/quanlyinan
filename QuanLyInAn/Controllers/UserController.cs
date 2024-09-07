using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyInAn.Data;
using QuanLyInAn.Models;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;

namespace QuanLyInAn.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users.ToListAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
           
            var claimsIdentity = User.Identity as ClaimsIdentity;

            if (claimsIdentity == null)
            {
                return Unauthorized("Không tìm thấy thông tin người dùng trong token.");
            }

           
            var subClaim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (subClaim == null)
            {
                return Unauthorized("Claim 'sub' không tồn tại trong token");
            }

           
            if (!int.TryParse(subClaim.Value, out int userIdFromToken))
            {
                return Unauthorized("Claim 'sub' không phải là số hợp lệ");
            }

           
            var roleClaim = claimsIdentity.FindFirst(ClaimTypes.Role);
            if (roleClaim == null)
            {
                return Unauthorized("Claim 'role' không tồn tại trong token");
            }

            if (!int.TryParse(roleClaim.Value, out int userRoleId))
            {
                return Unauthorized("Claim 'role' không phải là số hợp lệ");
            }

            
            if (id != userIdFromToken && userRoleId != 1)
            {
                return Unauthorized("Bạn không có quyền xem thông tin người dùng khác");
            }

           
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound("Người dùng không tồn tại");
            }

            return Ok(user);
        }
    }
}
