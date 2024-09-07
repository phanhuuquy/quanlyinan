using Microsoft.EntityFrameworkCore;
using QuanLyInAn.Data;
using System.Security.Claims;
using QuanLyInAn.Models;
using QuanLyInAn.DTOs;
namespace QuanLyInAn.Services
{
    public class ShippingService
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ShippingService(AppDbContext context, EmailService emailService, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _emailService = emailService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task AssignShipmentAsync(int projectId, int shipperId)
        {

            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == projectId && p.Progress == 75);

            if (project == null)
            {
                throw new ArgumentException("Dự án không tồn tại hoặc chưa sẵn sàng để phân công.");
            }


            var shipper = await _context.Users
                .Where(u => u.Id == shipperId && u.RoleId == 6 && u.DepartmentId == 1)
                .FirstOrDefaultAsync();

            if (shipper == null)
            {
                throw new ArgumentException("Nhân viên giao hàng không hợp lệ.");
            }

            // Lấy thông tin khách hàng
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == project.CustomerId);

            if (customer == null)
            {
                throw new ArgumentException("Khách hàng không tồn tại.");
            }

           
            var assignedShipment = new AssignedShipment
            {
                ProjectId = project.Id,
                ProjectName = project.ProjectName,
                CustomerId = customer.Id,
                CustomerName = customer.FullName,
                CustomerPhone = customer.PhoneNumber,
                CustomerAddress = customer.Address,
                ShipperId = shipperId
            };

            _context.AssignedShipments.Add(assignedShipment);
            await _context.SaveChangesAsync();
        }
        public async Task<List<CompletedProjectDto>> GetCompletedProjectsAsync()
        {
            var completedProjects = await _context.Projects
                .Where(p => p.Progress == 75)
                .Select(p => new CompletedProjectDto
                {
                    ProjectId = p.Id,
                    ProjectName = p.ProjectName,
                    Progress = p.Progress,
                    CustomerId = p.Customer.Id,
                    CustomerName = p.Customer.FullName,
                    CustomerAddress = p.Customer.Address,
                    CustomerEmail = p.Customer.Email
                }).ToListAsync();

            return completedProjects;
        }
        public async Task<List<ShipperDto>> GetShippersAsync()
        {
            var shippers = await _context.Users
                .Where(u => u.RoleId == 6 && u.DepartmentId.HasValue && u.DepartmentId == 1)
                .Select(u => new ShipperDto
                {
                    UserId = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    Address = u.Address 
                }).ToListAsync();

            return shippers;
        }



        public async Task<List<AssignedShipment>> GetAssignedShipmentsAsync(int shipperId)
        {
            return await _context.AssignedShipments
                .Where(s => s.ShipperId == shipperId)
                .ToListAsync();
        }
        public async Task ConfirmShipmentAsync(int projectId, int shipperId)
        {
            var shipment = await _context.AssignedShipments
                .FirstOrDefaultAsync(s => s.ProjectId == projectId && s.ShipperId == shipperId);

            if (shipment == null)
            {
                throw new ArgumentException("Giao hàng không tồn tại hoặc không thuộc về bạn.");
            }

            var project = await _context.Projects.Include(p => p.Customer).FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
            {
                throw new ArgumentException("Dự án không tồn tại.");
            }

           
            if (project.Progress == 100)
            {
                throw new InvalidOperationException("Dự án này đã được giao hàng và hoàn thành.");
            }

            // Cập nhật trạng thái giao hàng
            project.IsShipped = true;
            project.Progress = 100; 
            await _context.SaveChangesAsync();

            // Gửi thông báo emal cho khách hàng về việc dơn hang đã giao hàng thành công
            await _emailService.SendEmailAsync(project.Customer.Email, "Thông báo giao hàng thành công",
                $"Dự án {project.ProjectName} đã được giao hàng thành công.");

           
            _context.AssignedShipments.Remove(shipment);
            await _context.SaveChangesAsync();

            // Gửi thông báo cho leader và trưởng phòng giao hàng sau khi giao thành công
            var leader = await _context.Users.FirstOrDefaultAsync(u => u.RoleId == 4 && u.DepartmentId == 2);
            var shippingManager = await _context.Users.FirstOrDefaultAsync(u => u.RoleId == 3 && u.DepartmentId == 1);

            if (leader != null)
            {
                await _emailService.SendEmailAsync(leader.Email, "Thông báo giao hàng thành công", $"Dự án (ID: {projectId}) đã được giao hàng thành công.");
            }
            if (shippingManager != null)
            {
                await _emailService.SendEmailAsync(shippingManager.Email, "Thông báo giao hàng thành công", $"Dự án (ID: {projectId}) đã được giao hàng thành công.");
            }

            // Tạo thông báo và gửi cho leader của dự án và trưởng phòng giao hàng
            var leaderNotification = new Notification
            {
                UserId = leader?.Id ?? 0,
                Message = $"Dự án {project.ProjectName} đã được giao hàng thành công.",
                CreatedAt = DateTime.Now
            };
            var departmentHeadNotification = new Notification
            {
                UserId = shippingManager?.Id ?? 0,
                Message = $"Đơn hàng thuộc dự án {project.ProjectName} đã được giao hàng thành công.",
                CreatedAt = DateTime.Now
            };

            _context.Notifications.AddRange(leaderNotification, departmentHeadNotification);
            await _context.SaveChangesAsync();
        }

        public async Task<ShippingAssignmentSuggestionDto> GetShippingAssignmentSuggestionsAsync(int projectId)
        {
           
            var project = await _context.Projects
                .Where(p => p.Id == projectId && p.Progress == 75)
                .Include(p => p.Customer)
                .FirstOrDefaultAsync();

            if (project == null)
            {
                throw new ArgumentException("Dự án không tồn tại hoặc không đang chờ được giao.");
            }

            
            var projectAddress = project.Customer.Address;

          
            var shippers = await _context.Users
                .Where(u => u.RoleId == 6 && u.DepartmentId == 1)
                .ToListAsync();

            
            var matchingShippers = shippers.Where(s => s.Address == projectAddress).ToList();
            var nonMatchingShippers = shippers.Where(s => s.Address != projectAddress).ToList();

            var suggestionDto = new ShippingAssignmentSuggestionDto
            {
                MatchingShippers = matchingShippers.Select(s => new ShipperDto
                {
                    UserId = s.Id,
                    FullName = s.FullName,
                    Email = s.Email,
                    Address = s.Address
                }).ToList(),
                NonMatchingShippers = nonMatchingShippers.Select(s => new ShipperDto
                {
                    UserId = s.Id,
                    FullName = s.FullName,
                    Email = s.Email,
                    Address = s.Address
                }).ToList()
            };

            return suggestionDto;
        }



    }
}
