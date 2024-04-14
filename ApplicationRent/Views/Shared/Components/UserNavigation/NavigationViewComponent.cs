using ApplicationRent.Data.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ApplicationRent.Views.Shared.Components.Navigation
{
    [ViewComponent(Name = "UserNavigation")]
    public class NavigationViewComponent : ViewComponent
    {
        private readonly UserManager<ApplicationIdentityUser> _userManager;

        public NavigationViewComponent(UserManager<ApplicationIdentityUser> userManager)
        {
            _userManager = userManager;
        }

        /*public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var fullName = user?.FullNameUser ?? "Гость"; // Предполагая, что у вас есть свойство FullName
            return View("Default", fullName);
        }*/

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var fullName = user?.FullNameUser;

            var model = new UserNavigationViewModel
            {
                FullName = fullName,
                ProfileUrl = user != null ? Url.Action("Index", "UserProfile", new { userId = user.Id }) : null
            };

            return View("Default", model);
        }
    }
    public class UserNavigationViewModel
    {
        public string FullName { get; set; }
        public string ProfileUrl { get; set; }
    }
}
