using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyInAn.Models;
using QuanLyInAn.Services;
using System.Security.Claims;

namespace QuanLyInAn.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly CustomerService _customerService;

        public CustomerController(CustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCustomers()
        {
            var employeeId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var customers = await _customerService.GetAllCustomersAsync(employeeId);
            return Ok(customers);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomerById(int id)
        {
            var employeeId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var customer = await _customerService.GetCustomerByIdAsync(id, employeeId);
            if (customer == null) return NotFound();
            return Ok(customer);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCustomer([FromBody] Customer customer)
        {
            // Kiểm tra xem khách hàng có chứa ID không
            if (customer.Id != 0)
            {
                return BadRequest("Không cần cung cấp ID khi tạo mới.");
            }

            // Kiểm tra xem địa chỉ có bị trống không
            if (string.IsNullOrEmpty(customer.Address))
            {
                return BadRequest("Địa chỉ không được để trống.");
            }


            var employeeId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            customer.EmployeeId = employeeId;
            customer.ProjectCount = 0; 


            await _customerService.CreateCustomerAsync(customer);


            return CreatedAtAction(nameof(GetCustomerById), new { id = customer.Id }, customer);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] Customer customer)
        {
            // Kiểm tra xem địa chỉ có bị trống không
            if (string.IsNullOrEmpty(customer.Address))
            {
                return BadRequest("Địa chỉ không được để trống.");
            }

            var employeeId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (id != customer.Id || employeeId != customer.EmployeeId) return BadRequest();
            await _customerService.UpdateCustomerAsync(customer);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var employeeId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            await _customerService.DeleteCustomerAsync(id, employeeId);

            await _customerService.DecrementProjectCountAsync(id);

            return NoContent();
        }
    }
}
