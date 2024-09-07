using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyInAn.Data;
using QuanLyInAn.Models;

namespace QuanLyInAn.Services
{
    public class ProjectService
    {
        private readonly AppDbContext _context;
        private readonly CustomerService _customerService;
        private readonly EmailService _emailService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        // Constructor to initialize the services and IHttpContextAccessor
        public ProjectService(AppDbContext context, CustomerService customerService, EmailService emailService, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _customerService = customerService;
            _emailService = emailService;
            _httpContextAccessor = httpContextAccessor;
        }

        private ClaimsPrincipal User => _httpContextAccessor.HttpContext?.User;

        public async Task<List<Project>> GetAllProjectsAsync()
        {
            return await _context.Projects
                .Include(p => p.Designs)  // Ensure designs are included
                .ToListAsync();
        }

        public async Task<Project> GetProjectByIdAsync(int id)
        {
            return await _context.Projects
                .Include(p => p.Designs)  // Ensure designs are included
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task AddProjectAsync(Project project, int leaderId)
        {
            // Kiểm tra sự tồn tại của khách hàng
            var customer = await _context.Customers.FindAsync(project.CustomerId);
            if (customer == null)
            {
                throw new ArgumentException("CustomerId không hợp lệ.");
            }

            // Lấy thông tin người dùng hiện tại
            var user = User;
            var userRole = user?.FindFirst(ClaimTypes.Role)?.Value;
            var departmentId = user?.FindFirst("DepartmentId")?.Value;
            var employeeId = user?.FindFirstValue(ClaimTypes.NameIdentifier);

            // Kiểm tra quyền hạn của người dùng (chỉ nhân viên phòng Sales mới được phép tạo dự án)
            if (userRole != "2" || departmentId != "3")
            {
                throw new UnauthorizedAccessException("Chỉ những nhân viên thuộc phòng ban Sales mới được phép tạo dự án.");
            }

            // Xác thực thông tin người dùng
            if (!int.TryParse(employeeId, out int employeeIdInt))
            {
                throw new ArgumentException("Thông tin người dùng không hợp lệ.");
            }

            // Kiểm tra xem người dùng chỉ định có phải là Leader thuộc phòng ban Technical hay không
            var leader = await _context.Users
                .Where(u => u.Id == leaderId && u.RoleId == 4 && u.DepartmentId == 2)
                .FirstOrDefaultAsync();
            if (leader == null)
            {
                throw new ArgumentException("Người dùng được chỉ định không phải là Leader trong phòng ban Technical.");
            }

            // Thiết lập các thuộc tính cho dự án
            project.EmployeeId = employeeId;
            project.LeaderId = leaderId;
            project.Progress = 0;

            // Thêm dự án vào cơ sở dữ liệu và lưu lại
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();
        }




        public async Task AddDesignAsync(int projectId, int designerId, List<IFormFile> files)
        {
            var project = await _context.Projects
                .Include(p => p.Designs)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
            {
                throw new ArgumentException("Dự án không tồn tại.");
            }

            if (project.IsApproved)
            {
                throw new InvalidOperationException("Không thể thêm thiết kế mới vào dự án đã được phê duyệt.");
            }

            var designer = await _context.Users.FindAsync(designerId);

            if (designer == null || (designer.RoleId != 4 && designer.RoleId != 5))
            {
                throw new UnauthorizedAccessException("Người dùng không có quyền thêm thiết kế.");
            }

            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    // Đảm bảo thư mục tồn tại
                    var uploadsFolder = Path.Combine("wwwroot", "uploadsdesigns");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Tạo đường dẫn lưu file
                    var filePath = Path.Combine(uploadsFolder, file.FileName);

                    // Lưu file vào thư mục
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // Tạo đối tượng Design và lưu vào cơ sở dữ liệu
                    var design = new Design
                    {
                        ProjectId = projectId,
                        FilePath = filePath,
                        DesignerId = designerId,
                        IsApproved = false,
                        UploadedDate = DateTime.Now
                    };

                    _context.Designs.Add(design);
                    project.Designs.Add(design);
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log lỗi nếu cần
                Console.WriteLine($"Error saving changes: {ex.Message}");
                throw;
            }
        }

        public async Task ApproveDesignAsync(int projectId, int designId, int leaderId)
        {
            var project = await _context.Projects
                .Include(p => p.Designs)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
            {
                throw new ArgumentException("Dự án không tồn tại.");
            }

            if (project.IsApproved)
            {
                throw new InvalidOperationException("Dự án đã được phê duyệt, không thể phê duyệt thêm thiết kế.");
            }

            var designToApprove = project.Designs.FirstOrDefault(d => d.Id == designId);

            if (designToApprove == null)
            {
                throw new ArgumentException("Thiết kế không tồn tại.");
            }

            if (designToApprove.IsApproved)
            {
                throw new InvalidOperationException("Thiết kế này đã được phê duyệt trước đó.");
            }

            designToApprove.IsApproved = true;

           
            var unapprovedDesigns = project.Designs
                .Where(d => d.Id != designToApprove.Id && !d.IsApproved)
                .ToList();

            
            _context.Designs.RemoveRange(unapprovedDesigns);

            project.IsApproved = true;
            project.Progress = 25; 

            
            var approvedDesigners = new HashSet<int> { designToApprove.DesignerId };

            foreach (var design in project.Designs)
            {
                var designer = await _context.Users.FindAsync(design.DesignerId);
                if (designer != null)
                {
                    if (design.IsApproved)
                    {
                        // trả email về người được phee duyệt dự án 
                        string subject = "Thông báo về thiết kế của bạn";
                        string message = $"Dự án của bạn (projectId : {projectId} ) (designId: {design.Id}) đã được phê duyệt.";
                        await _emailService.SendEmailAsync(designer.Email, subject, message);
                        project.IsPrintingResourceSelected = true;
                    }
                    else
                    {
                        // trả về người không được phê duyệt
                        string subject = "Thiết kế bị từ chối";
                        string message = $"Thiết kế của bạn (projectId : {projectId} ) (designId: {design.Id}) không được phê duyệt và đã bị xóa.";
                        await _emailService.SendEmailAsync(designer.Email, subject, message);
                    }
                }
            }

            await _context.SaveChangesAsync();
        }
        public async Task<bool> CompletePrintingAsync(int projectId)
        {
            var project = await _context.Projects.Include(p => p.Customer).FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
            {
                throw new ArgumentException("Dự án không tồn tại.");
            }

            if (project.Progress == 75)
            {
                return false;
            }

            if (!project.IsPrintingResourceSelected)
            {
                throw new InvalidOperationException("Tài nguyên in ấn chưa được chọn.");
            }

            project.Progress = 75;

            _context.Projects.Update(project);
            await _context.SaveChangesAsync();

            // trả email thông báo cho khách hàng
            if (project.Customer != null && !string.IsNullOrEmpty(project.Customer.Email))
            {
                string subject = "Thông báo hoàn thành in ấn dự án";
                string body = $"Dự án của bạn có ID = {projectId} đã hoàn thành in ấn và đang chuẩn bị được giao hàng. Vui lòng để ý điện thoại vì sẽ có cuộc gọi từ nhân viên giao hàng.";
               
              
                await _emailService.SendEmailAsync(project.Customer.Email, subject, body);
                project.IsPrintingCompleted = true;
            }
            project.IsPrintingCompleted = true;
            return true;
        }
        public async Task DefinePrintingResourcesAsync(int projectId, int leaderId, Dictionary<int, int> resources)
        {
            var project = await _context.Projects
                .Include(p => p.Designs)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
            {
                throw new ArgumentException("Dự án không tồn tại.");
            }

            if (!project.IsApproved)
            {
                throw new InvalidOperationException("Thiết kế chưa được phê duyệt.");
            }

            var existingInfo = await _context.PrintingResourceInfos
                .FirstOrDefaultAsync(p => p.ProjectId == projectId);
            if (existingInfo != null)
            {
                throw new InvalidOperationException("Không thể xác nhận thêm .Thông tin in ấn đã được xác nhận cho dự án này.");
            }

            var user = User;
            var userRole = user?.FindFirst(ClaimTypes.Role)?.Value;
            var departmentId = user?.FindFirst("DepartmentId")?.Value;

            if (userRole != "4" || departmentId != "2")
            {
                throw new UnauthorizedAccessException("Chỉ leader mới có quyền xác định thông tin in ấn.");
            }

            var leader = await _context.Users
                .Where(u => u.Id == leaderId && u.RoleId == 4 && u.DepartmentId == 2)
                .FirstOrDefaultAsync();
            if (leader == null)
            {
                throw new ArgumentException("Người dùng không phải là Leader trong phòng ban Technical.");
            }

            var stocks = await _context.Stocks.ToListAsync();
            var stockDict = stocks.ToDictionary(s => s.Id);

            var missingResources = new List<string>();
            var insufficientResources = new List<string>();

            // Kiểm tra tài nguyên và số lượng đã nhập
            foreach (var stock in stocks)
            {
                if (!resources.ContainsKey(stock.Id))
                {
                    missingResources.Add(stock.Name);
                }
                else if (stock.IsConsumable)
                {
                    var quantity = resources[stock.Id];
                    if (stock.Quantity < quantity)
                    {
                        insufficientResources.Add($"Tài nguyên '{stock.Name}' không đủ.");
                    }
                    else
                    {
                        stock.Quantity -= quantity; // trừ số lượng cho tài nguyên tiêu hao
                    }
                }
               
            }

            if (missingResources.Count > 0)
            {
                throw new ArgumentException("Những tài nguyên bạn chưa nhập: " + string.Join(", ", missingResources));
            }

            if (insufficientResources.Count > 0)
            {
                throw new ArgumentException(string.Join(", ", insufficientResources));
            }

            
            project.Progress = 50;
            
            var printingResourceInfo = new PrintingResourceInfo
            {
                ProjectId = projectId,
                Resources = string.Join(", ", stocks.Select(s => $"ID={s.Id}: {resources.GetValueOrDefault(s.Id, 0)}")),
                IsCompleted = true
            };
            _context.PrintingResourceInfos.Add(printingResourceInfo);

            await _context.SaveChangesAsync();
        }




    }
}
