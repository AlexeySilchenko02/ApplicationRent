using Microsoft.AspNetCore.Identity;

namespace ApplicationRent.Data.Identity
{
    public class ApplicationIdentityUser : IdentityUser
    {
        public long ApplicationId { get; set; }
    }
}
