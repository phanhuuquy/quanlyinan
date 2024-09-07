namespace QuanLyInAn.DTOs
{
    public class CompletedProjectDto
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public int Progress { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerEmail { get; set; }
    }

}
