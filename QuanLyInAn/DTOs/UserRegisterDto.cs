using System.ComponentModel.DataAnnotations;

namespace QuanLyInAn.DTOs
{
    public class UserRegisterDto
    {
        [Required]
        public string FullName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public int DepartmentId { get; set; }

        [Required]
        public string Address { get; set; }
    }
}
