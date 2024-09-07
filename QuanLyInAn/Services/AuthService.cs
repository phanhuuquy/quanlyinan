using Microsoft.EntityFrameworkCore;
using QuanLyInAn.Data;
using QuanLyInAn.Models;
using QuanLyInAn.DTOs;
using QuanLyInAn.Helpers;
using System;
using System.Threading.Tasks;

namespace QuanLyInAn.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context;
        private readonly JwtHelper _jwtHelper;
        private readonly EmailService _emailService;

        public AuthService(AppDbContext context, JwtHelper jwtHelper, EmailService emailService)
        {
            _context = context;
            _jwtHelper = jwtHelper;
            _emailService = emailService;
        }

      
        public async Task<(bool Success, string Message, string Token)> RegisterUser(UserRegisterDto dto)
        {
          
            var existingUser = await _context.Users.SingleOrDefaultAsync(u => u.Email == dto.Email);
            if (existingUser != null)
                return (false, "Email đã được sử dụng", null);

          
            var department = await _context.Departments.FindAsync(dto.DepartmentId);
            if (department == null)
                return (false, "Phòng ban không tồn tại", null);

           
            var user = new User
            {
                FullName = dto.FullName,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                DateOfBirth = dto.DateOfBirth,
                DepartmentId = dto.DepartmentId,
                Address = dto.Address,  
                RoleId = 2  //(set role là eml..)
            };

           
            _context.Users.Add(user);

            
            department.MemberCount += 1;
            _context.Entry(department).State = EntityState.Modified;

     
            await _context.SaveChangesAsync();

            return (true, "Đăng ký thành công", null);
        }

       
        public async Task<(bool Success, string Message, string Token)> AuthenticateUser(UserLoginDto dto)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
                return (false, "Email hoặc mật khẩu sai", null);

            var token = _jwtHelper.GenerateToken(user);
            return (true, "Đăng nhập thành công", token);
        }

       
        public async Task<(bool Success, string Message)> SendForgotPasswordEmail(ForgotPasswordDto dto)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
                return (false, "Email không tồn tại");

            var confirmCode = Guid.NewGuid().ToString();
            var confirmEmail = new ConfirmEmail
            {
                UserId = user.Id,
                ConfirmCode = confirmCode,
                ExpiryTime = DateTime.UtcNow.AddHours(1),
                CreateTime = DateTime.UtcNow,
                IsConfirm = false
            };

            _context.ConfirmEmails.Add(confirmEmail);
            await _context.SaveChangesAsync();

            await _emailService.SendEmailAsync(user.Email, "Xác nhận mật khẩu", $"Mã xác nhận của bạn là: {confirmCode}");

            return (true, "Mã xác nhận đã được gửi tới email của bạn");
        }

     
        public async Task<(bool Success, string Message)> ResetUserPassword(ResetPasswordDto dto)
        {
            var confirmEmail = await _context.ConfirmEmails.SingleOrDefaultAsync(c => c.ConfirmCode == dto.ConfirmCode);
            if (confirmEmail == null || confirmEmail.ExpiryTime < DateTime.UtcNow || confirmEmail.IsConfirm)
                return (false, "Mã xác nhận không hợp lệ");

            var user = await _context.Users.SingleOrDefaultAsync(u => u.Id == confirmEmail.UserId);
            if (user == null)
                return (false, "Người dùng không tồn tại");

            user.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            _context.Users.Update(user);

            confirmEmail.IsConfirm = true;
            _context.ConfirmEmails.Update(confirmEmail);

            await _context.SaveChangesAsync();

            return (true, "Mật khẩu đã được cập nhật");
        }

        
        public async Task<(bool Success, string Message)> ChangeUserPassword(ChangePasswordDto dto)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
                return (false, "Người dùng không tồn tại");

            if (!BCrypt.Net.BCrypt.Verify(dto.OldPassword, user.Password))
                return (false, "Mật khẩu cũ sai");

            user.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            _context.Users.Update(user);

            await _context.SaveChangesAsync();

            return (true, "Mật khẩu đã được thay đổi");
        }

       
        private string GetDepartmentName(int departmentId)
        {
            return departmentId switch
            {
                1 => "Delivery",
                2 => "Technical",
                3 => "Sales",
                _ => "Unknown"
            };
        }
    }
}
