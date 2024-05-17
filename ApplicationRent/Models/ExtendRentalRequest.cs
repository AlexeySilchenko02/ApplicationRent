namespace ApplicationRent.Models
{
    public class ExtendRentalRequest
    {
        public int RentalId { get; set; }
        public DateTime NewEndDate { get; set; }
        public decimal Cost { get; set; }
    }
}
