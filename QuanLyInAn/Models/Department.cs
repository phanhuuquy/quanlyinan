
namespace QuanLyInAn.Models
{
    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? ManagerId { get; set; }
        public User? Manager { get; set; }
        public int MemberCount { get; set; }

    }
}
