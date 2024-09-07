namespace QuanLyInAn.Models
{
    public class Stock
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public bool IsConsumable { get; set; } 
    }
}
