using ApplicationRent.Data.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ApplicationRent.Views.Shared.Components.Admin
{
    [ViewComponent(Name = "AdminNavigation")]
    public class AdminNavigationViewComponent : ViewComponent
    {
        private readonly UserManager<ApplicationIdentityUser> _userManager;

        public AdminNavigationViewComponent(UserManager<ApplicationIdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            // Предполагая, что у класса ApplicationIdentityUser есть свойство IsAdmin
            var isAdmin = user?.Admin ?? false;

            return View("Default", isAdmin);
        }
    }
}
