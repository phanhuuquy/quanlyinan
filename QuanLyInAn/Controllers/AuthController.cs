using Microsoft.AspNetCore.Mvc;
using QuanLyInAn.DTOs;
using QuanLyInAn.Services;
using System.Threading.Tasks;

namespace QuanLyInAn.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDto dto)
        {
            var result = await _authService.RegisterUser(dto);

            if (result.Success)
                return Ok(new { Message = result.Message });
            else
                return BadRequest(new { Message = result.Message });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto dto)
        {
            var result = await _authService.AuthenticateUser(dto);

            if (result.Success)
                return Ok(new { Message = result.Message, Token = result.Token });
            else
                return BadRequest(new { Message = result.Message });
        }

        [HttpPost("forgotpassword")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
        {
            var result = await _authService.SendForgotPasswordEmail(dto);

            if (result.Success)
                return Ok(new { Message = result.Message });
            else
                return BadRequest(new { Message = result.Message });
        }

        [HttpPost("resetpassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        {
            // Kiểm tra confirmPassword có giống với newPassword không
            if (dto.NewPassword != dto.ConfirmPassword)
            {
                return BadRequest(new { Message = "Mật khẩu xác nhận không khớp với mật khẩu mới." });
            }


            var result = await _authService.ResetUserPassword(dto);

            if (result.Success)
                return Ok(new { Message = result.Message });
            else
                return BadRequest(new { Message = result.Message });
        }

        [HttpPost("changepassword")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
        {

            if (dto.NewPassword != dto.ConfirmPassword)
            {
                return BadRequest(new { Message = "Mật khẩu xác nhận không khớp với mật khẩu mới." });
            }


            var result = await _authService.ChangeUserPassword(dto);

            if (result.Success)
                return Ok(new { Message = result.Message });
            else
                return BadRequest(new { Message = result.Message });
        }

    }
}
