using Microsoft.EntityFrameworkCore;
using QuanLyInAn.Data;
using QuanLyInAn.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyInAn.Services
{
    public class DepartmentService
    {
        private readonly AppDbContext _context;
        private const int SalesTeamId = 3; // ID Sale

        public DepartmentService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Department>> GetAllDepartmentsAsync()
        {
            return await _context.Departments.ToListAsync();
        }

        public async Task<Department> GetDepartmentByIdAsync(int id)
        {
            return await _context.Departments.FindAsync(id);
        }

        public async Task AddDepartmentAsync(Department department)
        {
            _context.Departments.Add(department);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateDepartmentAsync(Department department)
        {
            _context.Departments.Update(department);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteDepartmentAsync(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department != null)
            {
                _context.Departments.Remove(department);
                await _context.SaveChangesAsync();
            }
        }

       
        public async Task TransferDepartmentAsync(int userId, int newDepartmentId, int newEmployeeId)
        {
            
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
               
                if (user.DepartmentId == SalesTeamId)
                {
                   
                    var customers = await _context.Customers.Where(c => c.EmployeeId == userId).ToListAsync();

                    
                    foreach (var customer in customers)
                    {
                        customer.EmployeeId = newEmployeeId;
                    }

                   
                    await _context.SaveChangesAsync();
                }

                
                user.DepartmentId = newDepartmentId;
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new KeyNotFoundException("Không tìm thấy nhân viên với ID đã cung cấp.");
            }
        }
    }
}
