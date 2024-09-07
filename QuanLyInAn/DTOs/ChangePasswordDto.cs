using System.ComponentModel.DataAnnotations;

namespace QuanLyInAn.DTOs
{
    public class ChangePasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string OldPassword { get; set; }
        [Required]
        public string NewPassword { get; set; }

        public string ConfirmPassword { get; set; }
    }
}
