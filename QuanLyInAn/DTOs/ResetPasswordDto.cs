using System.ComponentModel.DataAnnotations;

namespace QuanLyInAn.DTOs
{
    public class ResetPasswordDto
    {
        [Required]
        public string ConfirmCode { get; set; }
        [Required]
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
