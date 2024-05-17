using ApplicationRent.Data.Identity;

namespace ApplicationRent.Models
{
    public class PlaceDetailsViewModel
    {
        public Place Place { get; set; }
        public ReviewViewModel NewReview { get; set; }
        public List<Review> Reviews { get; set; }
        public double AverageRating { get; set; }
    }
}
