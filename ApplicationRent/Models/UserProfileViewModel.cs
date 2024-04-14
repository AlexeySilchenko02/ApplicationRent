using ApplicationRent.Data.Identity;

namespace ApplicationRent.Models
{
    public class UserProfileViewModel
    {
        public ApplicationIdentityUser User { get; set; }
        public List<Rental> Rentals { get; set; }
    }
}
