using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyInAn.Data;
using QuanLyInAn.Models;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyInAn.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "1")]
    public class DepartmentController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DepartmentController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateDepartment(Department department, int? employeeId)
        {
            if (ModelState.IsValid)
            {
                department.MemberCount = 0;

                if (employeeId.HasValue)
                {
                    var employee = await _context.Users.FindAsync(employeeId.Value);
                    if (employee == null)
                        return NotFound("Nguoi dung khong ton tai");

                    if (employee.RoleId != 2)
                        return BadRequest("Chi nhung nhan vien co role la employee moi duoc lam truong phong");

                    employee.DepartmentId = department.Id;
                    employee.RoleId = 3; 
                    department.ManagerId = employeeId.Value;
                    department.MemberCount += 1;

                    _context.Entry(employee).State = EntityState.Modified;
                }

                _context.Departments.Add(department);
                await _context.SaveChangesAsync();
                return Ok(department);
            }
            return BadRequest(ModelState);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDepartment(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department == null)
                return NotFound("Phong ban khong ton tai");

         
            await UpdateMemberCount(id);

            return Ok(department);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDepartment(int id, Department department)
        {
            if (id != department.Id)
                return BadRequest("ID phòng ban không đúng");

            var existingDepartment = await _context.Departments.FindAsync(id);
            if (existingDepartment == null)
                return NotFound("Phòng ban không tồn tại");

            existingDepartment.Name = department.Name;
            existingDepartment.Description = department.Description;
            existingDepartment.ManagerId = department.ManagerId;

            _context.Entry(existingDepartment).State = EntityState.Modified;
            await _context.SaveChangesAsync();

           
            return Ok(new
            {
                Message = "Cập nhật phòng ban thành công",
                Department = existingDepartment
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department == null)
                return NotFound("Phòng ban không tồn tại");

            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Xóa phòng ban thành công" });
        }


        [HttpPut("{id}/manager/{employeeId}")]
        public async Task<IActionResult> ChangeDepartmentHead(int id, int employeeId)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department == null)
                return NotFound("Phong ban khong ton tai");

            var employee = await _context.Users.FindAsync(employeeId);
            if (employee == null)
                return NotFound("Nguoi dung khong ton tai");


            if (employee.RoleId != 2)
                return BadRequest("Chi nhung nhan vien co role la employee moi duoc lam truong phong");

            // cập nhật lại phòngg ban neeu phòng ban hiện tại của employee không phải phòng ban này, 
            if (employee.DepartmentId.HasValue && employee.DepartmentId.Value != id)
            {
                var oldDepartment = await _context.Departments.FindAsync(employee.DepartmentId.Value);
                if (oldDepartment != null)
                {
                    oldDepartment.MemberCount -= 1;
                    _context.Entry(oldDepartment).State = EntityState.Modified;
                }
            }

       
            employee.DepartmentId = id;
            employee.RoleId = 3; 
            _context.Entry(employee).State = EntityState.Modified;

            
            department.ManagerId = employeeId;
            department.MemberCount += 1;
            _context.Entry(department).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return Ok("Cap nhat truong phong thanh cong");
        }

        [HttpPut("{id}/transfer/{employeeId}")]
        public async Task<IActionResult> TransferEmployee(int id, int employeeId)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department == null)
                return NotFound("Phong ban khong ton tai");

            var employee = await _context.Users.FindAsync(employeeId);
            if (employee == null)
                return NotFound("Nguoi dung khong ton tai");


            if (employee.DepartmentId.HasValue && employee.DepartmentId.Value != id)
            {
                var oldDepartment = await _context.Departments.FindAsync(employee.DepartmentId.Value);
                if (oldDepartment != null)
                {
                    oldDepartment.MemberCount -= 1;
                    _context.Entry(oldDepartment).State = EntityState.Modified;
                }
            }

            // Cập nhaat lại phòng ban mới
            employee.DepartmentId = id;
            _context.Entry(employee).State = EntityState.Modified;


            department.MemberCount += 1;
            _context.Entry(department).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return Ok("Chuyen nhan vien thanh cong");
        }

        private async Task UpdateMemberCount(int departmentId)
        {
            var department = await _context.Departments.FindAsync(departmentId);
            if (department != null)
            {
                department.MemberCount = await _context.Users.CountAsync(u => u.DepartmentId == departmentId);
                _context.Entry(department).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDepartments()
        {
            var departments = await _context.Departments
                .Select(d => new
                {
                    d.Id,
                    d.Name,
                    d.Description,
                    MemberCount = _context.Users.Count(u => u.DepartmentId == d.Id) 
                })
                .ToListAsync();

            return Ok(departments);
        }
    }
}
