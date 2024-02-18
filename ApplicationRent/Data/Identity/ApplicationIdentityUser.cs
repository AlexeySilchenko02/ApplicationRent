using Microsoft.AspNetCore.Identity;

namespace ApplicationRent.Data.Identity
{
    public class ApplicationIdentityUser : IdentityUser
    {
        //public string FullName { get; set; }
        public string FullNameUser { get; set; }
        // public long ApplicationId { get; set; }
    }
}
