using ApplicationRent.App_data;
using ApplicationRent.Data;
using ApplicationRent.Data.Identity;
using ApplicationRent.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApplicationRent.Controllers
{
    public class UserProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationIdentityUser> _userManager;

        public UserProfileController(ApplicationDbContext context, UserManager<ApplicationIdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return View("Error"); // Показываем страницу ошибки, если пользователь не найден
            }

            var today = DateTime.Now; // Получаем текущую дату

            var rentals = await _context.Rentals
                                        .Where(r => r.UserId == user.Id && r.EndRent > today) // Фильтруем по ID пользователя и дате окончания аренды
                                        .Include(r => r.Place) // Добавляем данные о месте
                                        .ToListAsync();

            var model = new UserProfileViewModel
            {
                Rentals = rentals,
                User = user
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRentalEndDate(int rentalId, DateTime newEndDate, [FromServices] FirebaseService firebaseService)
        {
            var rental = await _context.Rentals
                                       .Include(r => r.Place)
                                       .FirstOrDefaultAsync(r => r.Id == rentalId);
            if (rental == null)
            {
                return NotFound();
            }

            bool updateFirebase = false;  // Флаг для отслеживания, нужно ли обновлять Firebase

            // Обновляем дату окончания аренды в Rental
            if (newEndDate > rental.EndRent)
            {
                rental.EndRent = newEndDate;
                updateFirebase = true;  // Помечаем, что нужно обновить Firebase
            }

            // Обновляем дату окончания аренды и статус в Place, если это необходимо
            if (rental.Place != null && newEndDate > rental.Place.EndRent)
            {
                rental.Place.EndRent = newEndDate;
                rental.Place.InRent = true; // Обновляем статус на "в аренде"
                updateFirebase = true;  // Помечаем, что нужно обновить Firebase
            }

            // Сохраняем изменения в локальной базе данных
            await _context.SaveChangesAsync();

            // Если были изменения, обновляем данные в Firebase
            if (updateFirebase)
            {
                await firebaseService.AddOrUpdateRental(rental);  // Обновляем данные аренды в Firebase
                if (rental.Place != null)
                {
                    await firebaseService.AddOrUpdatePlace(rental.Place);  // Обновляем данные места в Firebase
                }
            }

            return RedirectToAction(nameof(Index)); // Возвращаем пользователя на страницу индекса
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUserProfile(ApplicationIdentityUser model)
        {
            if (!ModelState.IsValid)
            {
                // Возвращаем ошибку 400 с сообщением о неверных данных
                return BadRequest(new { errors = ModelState });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return View("Error"); // Вывод страницы ошибки, если пользователь не найден
            }

            // Проверяем, существует ли пользователь с таким email
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null && existingUser.Id != user.Id)
            {
                return BadRequest(new { errors = new { Email = "Пользователь с таким email уже существует." } });
            }

            // Обновляем данные
            user.FullNameUser = model.FullNameUser;
            user.PhoneNumber = model.PhoneNumber;
            user.Email = model.Email;
            user.UserName = model.Email; // Обновляем UserName согласно email

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return Ok(); // Возвращаем успешный статус 200
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description); // Добавляем ошибки в ModelState
                }
                // Возвращаем ошибку 400 с сообщением о неверных данных
                return BadRequest(new { errors = ModelState });
            }
        }


        //Страница смены пароля
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return View("Error");
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (changePasswordResult.Succeeded)
            {
                TempData["SuccessMessage"] = "Пароль успешно изменен.";
                //return RedirectToAction("Index");
                return View(model);
            }
            else
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }
        }
    }
}
