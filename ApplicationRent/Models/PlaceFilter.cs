namespace ApplicationRent.Models
{
    public class PlaceFilter
    {
        public string Category { get; set; }
        public bool? InRent { get; set; }
        public string PriceSort { get; set; }
        public string SizeSort { get; set; }
    }
}
