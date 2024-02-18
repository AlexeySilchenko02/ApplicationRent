using ApplicationRent.Data.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ApplicationRent.Views.Shared.Components.Navigation
{
    public class NavigationViewComponent : ViewComponent
    {
        private readonly UserManager<ApplicationIdentityUser> _userManager;

        public NavigationViewComponent(UserManager<ApplicationIdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var fullName = user?.FullNameUser ?? "Гость"; // Предполагая, что у вас есть свойство FullName
            return View("Default", fullName);
        }
    }
}
