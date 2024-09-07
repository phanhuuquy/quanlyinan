using System.ComponentModel.DataAnnotations;

namespace QuanLyInAn.DTOs
{
    public class AssignRoleDto
    {
        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public int RoleId { get; set; }
    }
}
