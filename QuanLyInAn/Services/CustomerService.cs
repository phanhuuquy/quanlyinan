using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuanLyInAn.Data;
using QuanLyInAn.Models;

namespace QuanLyInAn.Services
{
    public class CustomerService
    {
        private readonly AppDbContext _context;

        public CustomerService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Customer>> GetAllCustomersAsync(int employeeId)
        {
            return await _context.Customers
                .Where(c => c.EmployeeId == employeeId)
                .ToListAsync();
        }

        public async Task<Customer> GetCustomerByIdAsync(int id, int employeeId)
        {
            return await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == id && c.EmployeeId == employeeId);
        }

        public async Task CreateCustomerAsync(Customer customer)
        {
            // Đặt ProjectCount mặc định là 0 
            customer.ProjectCount = 0;

            // Kiểm tra xem địa chỉ có bị trống không // địa chr phải bắt buộc
            if (string.IsNullOrEmpty(customer.Address))
            {
                throw new ArgumentException("Địa chỉ không được để trống.");
            }

            
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCustomerAsync(Customer customer)
        {
            
            if (string.IsNullOrEmpty(customer.Address))
            {
                throw new ArgumentException("Địa chỉ không được để trống.");
            }

            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCustomerAsync(int id, int employeeId)
        {
            var customer = await GetCustomerByIdAsync(id, employeeId);
            if (customer != null)
            {
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
            }
        }

        public async Task IncrementProjectCountAsync(int customerId)
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer != null)
            {
                customer.ProjectCount++;
                _context.Customers.Update(customer);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DecrementProjectCountAsync(int customerId)
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null)
            {
                throw new ArgumentException("Khách hàng không tồn tại.");
            }

            customer.ProjectCount = Math.Max(0, customer.ProjectCount - 1);
            await _context.SaveChangesAsync();
        }
    }
}
