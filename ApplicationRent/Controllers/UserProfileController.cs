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
            List<string> errors = new List<string>();

            if (!ModelState.IsValid)
            {
                errors = ModelState.Values.SelectMany(v => v.Errors.Select(b => b.ErrorMessage)).ToList();
                return Json(new { success = false, errors = errors });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return View("Error");
            }

            // Проверка на совпадение нового пароля и подтверждения
            if (model.NewPassword != model.ConfirmPassword)
            {
                errors.Add("Пароли не совпадают");
                return Json(new { success = false, errors = errors });
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (changePasswordResult.Succeeded)
            {
                return Json(new { success = true });
            }
            else
            {
                errors.AddRange(changePasswordResult.Errors.Select(e => e.Description));
                return Json(new { success = false, errors = errors });
            }
        }

        [HttpGet]
        public IActionResult TopUpBalance()
        {
            return View(new TopUpBalanceViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> TopUpBalance(TopUpBalanceViewModel model)
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

            user.Balance += model.Amount;
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                var transaction = new TransactionHistory
                {
                    UserId = user.Id,
                    Amount = model.Amount,
                    TransactionDate = DateTime.UtcNow.AddHours(3)
                };

                _context.TransactionHistories.Add(transaction);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> TransactionHistory()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return View("Error");
            }

            var transactions = await _context.TransactionHistories
                                             .Where(t => t.UserId == user.Id)
                                             .OrderByDescending(t => t.TransactionDate)
                                             .ToListAsync();

            return View(transactions);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRentalEndDate([FromBody] ExtendRentalRequest request, [FromServices] FirebaseService firebaseService)
        {
            var rental = await _context.Rentals
                                       .Include(r => r.Place)
                                       .FirstOrDefaultAsync(r => r.Id == request.RentalId);
            if (rental == null)
            {
                return Json(new { success = false, error = "Аренда не найдена" });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, error = "Пользователь не найден" });
            }

            if (user.Balance < request.Cost)
            {
                return Json(new { success = false, error = "Недостаточно средств на балансе" });
            }

            bool updateFirebase = false;  // Флаг для отслеживания, нужно ли обновлять Firebase

            // Обновляем дату окончания аренды в Rental
            if (request.NewEndDate > rental.EndRent)
            {
                rental.EndRent = request.NewEndDate;
                updateFirebase = true;  // Помечаем, что нужно обновить Firebase
            }

            // Обновляем дату окончания аренды и статус в Place, если это необходимо
            if (rental.Place != null && request.NewEndDate > rental.Place.EndRent)
            {
                rental.Place.EndRent = request.NewEndDate;
                rental.Place.InRent = true; // Обновляем статус на "в аренде"
                updateFirebase = true;  // Помечаем, что нужно обновить Firebase
            }

            // Вычитаем стоимость продления из баланса пользователя
            user.Balance -= request.Cost;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return Json(new { success = false, error = "Ошибка при обновлении баланса пользователя" });
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

            return Json(new { success = true });
        }
    }
}
