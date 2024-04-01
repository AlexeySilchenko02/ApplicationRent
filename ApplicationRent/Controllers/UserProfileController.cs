using ApplicationRent.Data.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ApplicationRent.Controllers
{
    public class UserProfileController : Controller
    {
        private readonly UserManager<ApplicationIdentityUser> _userManager;

        public UserProfileController(UserManager<ApplicationIdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return View("Error"); // Возвращайте представление ошибки или перенаправляйте, если пользователь не найден
            }

            return View(user);
        }
    }
}
