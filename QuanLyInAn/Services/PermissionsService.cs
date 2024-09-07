using Microsoft.EntityFrameworkCore;
using QuanLyInAn.Data;
using QuanLyInAn.Models;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyInAn.Services
{
    public class PermissionsService
    {
        private readonly AppDbContext _context;

        public PermissionsService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, string Message)> AssignRoleToUserAsync(int employeeId, int roleId)
        {
            // Kiểm tra người dùng có tồn tại khoong
            var user = await _context.Users.FindAsync(employeeId);
            if (user == null)
                return (false, "Người dùng không tồn tại.");

            // Kiểm traa xem vai trò có hợp lệ không
            var validRoleIds = new[] { 4, 5, 6 }; // Ví dụ vai trò hợp lệ
            if (!validRoleIds.Contains(roleId))
                return (false, "Vai trò không hợp lệ.");

            // Kiểm tra phòng ban của nguo dùng và cấp quyền tương ứng
            if ((roleId == 4 || roleId == 5) && user.DepartmentId != 2) 
            {
                return (false, "Người dùng không thuộc phòng ban Technical.");
            }

            if (roleId == 6 && user.DepartmentId != 1) 
            {
                return (false, "Người dùng không thuộc phòng ban Delivery.");
            }

           
            user.RoleId = roleId;
            _context.Users.Update(user);

            
            await _context.SaveChangesAsync();
            return (true, "Cấp quyền thành công.");
        }
    }
}
