using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuanLyInAn.Services;
using System.Security.Claims;

namespace QuanLyInAn.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShippingController : ControllerBase
    {
        private readonly ShippingService _shippingService;

        public ShippingController(ShippingService shippingService)
        {
            _shippingService = shippingService;
        }

        [HttpPost("{projectId}/assign/{shipperId}")]
        [Authorize(Roles = "3")] // quyeen trưởng phòng giao hàng
        public async Task<IActionResult> AssignShipper(int projectId, int shipperId)
        {
            try
            {
                await _shippingService.AssignShipmentAsync(projectId, shipperId);
                return Ok(new { Message = "Phân công nhân viên giao hàng thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
        [HttpGet("completed-projects")]
        [Authorize(Roles = "3")] 
        public async Task<IActionResult> GetCompletedProjects()
        {
            try
            {
                
                var completedProjects = await _shippingService.GetCompletedProjectsAsync();
                return Ok(completedProjects);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
        }

        [HttpGet("shippers")]
        [Authorize(Roles = "3")]
        public async Task<IActionResult> GetShippers()
        {
            try
            {
                
                var shippers = await _shippingService.GetShippersAsync();
                return Ok(shippers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
        }


        [HttpGet("assigned")]
        [Authorize(Roles = "6")] 
        public async Task<IActionResult> GetAssignedShipments()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int userId))
            {
                return BadRequest(new { Message = "ID người dùng không hợp lệ." });
            }

            var shipments = await _shippingService.GetAssignedShipmentsAsync(userId);
            return Ok(shipments);
        }
        [HttpPost("{projectId}/confirm")]
        [Authorize(Roles = "6")] 
        public async Task<IActionResult> ConfirmShipment(int projectId)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int userId))
            {
                return BadRequest(new { Message = "ID người dùng không hợp lệ." });
            }

            try
            {
                await _shippingService.ConfirmShipmentAsync(projectId, userId);
                return Ok(new { Message = "Xác nhận giao hàng thành công." });
            }
            catch (InvalidOperationException ex)
            {
                
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
        [HttpGet("suggest-shipping-assignment/{projectId}")]
        [Authorize(Roles = "3")] 
        public async Task<IActionResult> GetShippingAssignmentSuggestions(int projectId)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var roleClaim = User.FindFirstValue(ClaimTypes.Role);

            
            if (int.TryParse(roleClaim, out int roleId) && roleId == 3)
            {
                var suggestions = await _shippingService.GetShippingAssignmentSuggestionsAsync(projectId);
                return Ok(suggestions);
            }

            return Unauthorized("Bạn không có quyền truy cập vào chức năng này.");
        }


    }


}
