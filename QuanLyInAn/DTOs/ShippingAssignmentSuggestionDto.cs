namespace QuanLyInAn.DTOs
{
    public class ShippingAssignmentSuggestionDto
    {
        public List<ShipperDto> MatchingShippers { get; set; }
        public List<ShipperDto> NonMatchingShippers { get; set; }
    }

}
