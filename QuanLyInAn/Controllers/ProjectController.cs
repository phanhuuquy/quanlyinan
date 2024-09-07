using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyInAn.Data;
using QuanLyInAn.Models;
using QuanLyInAn.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace QuanLyInAn.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly AppDbContext _context; 
        private readonly ProjectService _projectService;
        private readonly ShippingService _shippingService;

        public ProjectController(AppDbContext context, ProjectService projectService, ShippingService shippingService)
        {
            _context = context;
            _projectService = projectService;
            _shippingService = shippingService;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateProject([FromForm] Project project, [FromForm] int leaderId, [FromForm] IFormFile image)
        {
            try
            {
  
                if (image != null && image.Length > 0)
                {
                    var uploadsFolder = Path.Combine("wwwroot", "uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var imagePath = Path.Combine(uploadsFolder, Guid.NewGuid().ToString() + Path.GetExtension(image.FileName));
                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }
                    project.ImagePath = imagePath; 
                }

       
                Console.WriteLine($"Received project: {project}");
                Console.WriteLine($"Received leaderId: {leaderId}");

           
                await _projectService.AddProjectAsync(project, leaderId);

                return Ok(project);
            }
            catch (UnauthorizedAccessException)
            {
                return StatusCode(StatusCodes.Status403Forbidden, "Bạn không có quyền thực hiện hành động này.");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }


        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllProjects()
        {
            if (!IsAuthorizedToViewProjects())
            {
                return StatusCode(StatusCodes.Status403Forbidden, "Bạn không có quyền xem tất cả các dự án.");
            }

            var projects = await _projectService.GetAllProjectsAsync();
            return Ok(projects);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProjectById(int id)
        {
            if (!IsAuthorizedToViewProjects())
            {
                return StatusCode(StatusCodes.Status403Forbidden, "Bạn không có quyền xem thông tin của dự án.");
            }

            var project = await _projectService.GetProjectByIdAsync(id);
            if (project == null)
                return NotFound("Dự án không tồn tại.");

            return Ok(project);
        }

        [Authorize(Roles = "4,5")]
        [HttpPost("{projectId}/designs")]
        public async Task<IActionResult> AddDesigns(int projectId, [FromForm] List<IFormFile> files)
        {
            var designerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (files == null || !files.Any())
            {
                return BadRequest("No files uploaded.");
            }

            try
            {
                await _projectService.AddDesignAsync(projectId, designerId, files);
                return Ok("Thiết kế đã được tải lên thành công.");
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [Authorize(Roles = "4")]
        [HttpPost("{projectId}/designs/{designId}/approve")]
        public async Task<IActionResult> ApproveDesign(int projectId, int designId)
        {
            var leaderId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            try
            {
                await _projectService.ApproveDesignAsync(projectId, designId, leaderId);
                return Ok(new { Message = "Thiết kế đã được phê duyệt." });
            }
            catch (UnauthorizedAccessException)
            {
                return StatusCode(StatusCodes.Status403Forbidden, "Bạn không có quyền phê duyệt thiết kế.");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        [HttpPost("{projectId}/define-printing-resources")]
        [Authorize(Roles = "4")]
        public async Task<IActionResult> DefinePrintingResources(int projectId, [FromBody] Dictionary<int, int> resources)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int userId))
            {
                return BadRequest(new { Message = "ID người dùng không hợp lệ." });
            }

            try
            {
                await _projectService.DefinePrintingResourcesAsync(projectId, userId, resources);
                return Ok(new { Message = "Thông tin in ấn đã được xác định." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }


        [HttpPost("{projectId}/complete-printing")]
        [Authorize(Roles = "4")] 
        public async Task<IActionResult> CompletePrinting(int projectId)
        {
            try
            {
               
                var result = await _projectService.CompletePrintingAsync(projectId);

                if (result)
                {
                    return Ok(new { Message = "in Ấn đã được hoàn thành. Email thông báo đã được gửi cho khách hàng." });

                }
                else
                {
                    return BadRequest(new { Message = "Dự án đã hoàn thành in ấn trước đó, không thể hoàn thành lại." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }


        
        private bool IsAuthorizedToViewProjects()
        {
            var departmentId = User.FindFirst("DepartmentId")?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            return departmentId == "2" && (role == "1" || role == "4" || role == "5");
        }
    }
}
