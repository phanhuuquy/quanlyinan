using System.ComponentModel.DataAnnotations;

namespace QuanLyInAn.Models
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        public string Gender { get; set; }
        public string Address { get; set; }
        public int ProjectCount { get; set; }

        public int? EmployeeId { get; set; }
        public User? Employee { get; set; } 

        public List<Project> Projects { get; set; } = new List<Project>();
    }
}
