using QuanLyInAn.Models;

public class Project
{
    public int Id { get; set; }
    public string ProjectName { get; set; }
    public string RequestDescriptionFromCustomer { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime ExpectedEndDate { get; set; }
    public string EmployeeId { get; set; }
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public int LeaderId { get; set; }
    public int Progress { get; set; }
    public bool IsApproved { get; set; }
    public List<Design> Designs { get; set; } = new List<Design>();
    public string ImagePath { get; set; }
    public bool IsPrintingCompleted { get; set; }
    public int ShipperId { get; set; }
    public bool IsShipped { get; set; }
    public bool IsPrintingResourceSelected { get; set; }
}
