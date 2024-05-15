namespace ApplicationRent.Models
{
    public class RentCalculatorViewModel
    {
        public int PlaceId { get; set; }
        public DateTime StartDate { get; set; } = DateTime.Today;
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(1);
        public decimal TotalCost { get; set; }
    }
}
