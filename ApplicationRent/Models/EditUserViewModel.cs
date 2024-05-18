using System.ComponentModel.DataAnnotations;

namespace ApplicationRent.Models
{
    public class EditUserViewModel
    {
        public string Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        [Required]
        public string FullNameUser { get; set; }

        public bool Admin { get; set; }
        public decimal Balance { get; set; }
    }
}
