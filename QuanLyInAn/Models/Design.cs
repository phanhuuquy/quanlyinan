using QuanLyInAn.Models;

public class Design
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string FilePath { get; set; }
    public int DesignerId { get; set; }
    public bool IsApproved { get; set; }
    public DateTime UploadedDate { get; set; }

    public Project Project { get; set; }
}
