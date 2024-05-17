using Microsoft.AspNetCore.Identity;

namespace ApplicationRent.Data.Identity
{
    public class ApplicationIdentityUser : IdentityUser
    {
        public string FullNameUser { get; set; }
        public bool Admin {  get; set; } = false;
        public decimal Balance { get; set; }
    }
}
