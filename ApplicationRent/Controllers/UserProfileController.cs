using ApplicationRent.App_data;
using ApplicationRent.Data;
using ApplicationRent.Data.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApplicationRent.Controllers
{
    public class UserProfileController : Controller
    {
        private readonly UserManager<ApplicationIdentityUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly FirebaseService _firebaseService;

        public UserProfileController(UserManager<ApplicationIdentityUser> userManager, ApplicationDbContext context, FirebaseService firebaseService)
        {
            _userManager = userManager;
            _context = context;
            _firebaseService = firebaseService;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return View("Error");
            }

            var today = DateTime.Now;
            var rentedPlaces = _context.Rentals
                .Where(r => r.UserId == user.Id && r.EndRent > today)
                .Select(r => new RentedPlaceViewModel // Используйте явно определенный класс вместо анонимного объекта
                {
                    Name = r.Place.Name,
                    StartRent = r.StartRent,
                    EndRent = r.EndRent
                })
                .ToList();

            ViewBag.RentedPlaces = rentedPlaces;
            ViewBag.FullNameUser = user.FullNameUser;

            return View();
        }

    }
    public class RentedPlaceViewModel
    {
        public int RentalId { get; set; } // ID арендованного места
        public string Name { get; set; }
        public DateTime StartRent { get; set; }
        public DateTime EndRent { get; set; }
    }
}
