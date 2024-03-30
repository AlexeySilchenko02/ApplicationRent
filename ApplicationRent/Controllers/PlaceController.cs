using ApplicationRent.Data.Identity;
using ApplicationRent.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using ApplicationRent.App_data;


namespace ApplicationRent.Controllers
{
    public class PlaceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationIdentityUser> _userManager; //для передачи статуса админа
        private readonly FirebaseService _firebaseService;

        public PlaceController(ApplicationDbContext context, UserManager<ApplicationIdentityUser> userManager, FirebaseService firebaseService)
        {
            _context = context;
            _userManager = userManager;
            _firebaseService = firebaseService;
        }

        //Вызов страницы Index
        public async Task<IActionResult> Index()
        {
            var places = await _context.Places.ToListAsync();
            var today = DateTime.Now;

            foreach (var place in places)
            {
                // Проверка, истекла ли дата аренды, и обновление свойства InRent
                if (place.EndRent < today)
                {
                    place.InRent = false;
                    // Обновление в Firebase
                    await _firebaseService.AddOrUpdatePlace(place);
                }
                else
                {
                    place.InRent = true;
                    // Обновление в Firebase
                    await _firebaseService.AddOrUpdatePlace(place);
                }
            }
            // Сохранение изменений, если они есть
            await _context.SaveChangesAsync();

            // Передача статуса пользователя
            var user = await _userManager.GetUserAsync(User);
            var isAdmin = user?.Admin ?? false;
            ViewBag.IsAdmin = isAdmin;

            return View(places);
        }

        //Вызов страници для отображения детальной информации
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var place = await _context.Places
                .FirstOrDefaultAsync(m => m.Id == id);
            if (place == null)
            {
                return NotFound();
            }

            return View(place);
        }

        // Вызов страницы аренды
        public async Task<IActionResult> Rent(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var place = await _context.Places.FindAsync(id);
            if (place == null)
            {
                return NotFound();
            }

            // Предоставляем информацию о месте и диапазоне дат для аренды
            var rentViewModel = new RentViewModel
            {
                PlaceId = place.Id,
                StartRent = DateTime.Now,
                EndRent = DateTime.Now.AddDays(1) // Пример начальных значений
            };

            return View(rentViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmRent(RentViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    // Не удалось получить пользователя, возможно, следует добавить обработку ошибок
                    return Unauthorized();
                }

                // Создаем новую аренду с Email пользователя
                var rental = new Rental
                {
                    UserId = user.Id,
                    PlaceId = model.PlaceId,
                    StartRent = model.StartRent,
                    EndRent = model.EndRent,
                    UserEmail = user.Email // Добавляем Email пользователя
                };

                _context.Add(rental);

                // Находим место, которое арендуется, и обновляем его данные
                var place = await _context.Places.FirstOrDefaultAsync(p => p.Id == model.PlaceId);
                if (place != null)
                {
                    place.StartRent = model.StartRent;
                    place.EndRent = model.EndRent;
                    place.InRent = true; // Обновляем статус места как занято
                    _context.Update(place);
                }

                await _context.SaveChangesAsync();

                // Обновление в Firebase для места
                await _firebaseService.AddOrUpdatePlace(place);

                // Сохранение информации об аренде в Firebase
                await _firebaseService.AddOrUpdateRental(rental);

                return RedirectToAction(nameof(Index));
            }

            // В случае ошибки возвращаем пользователя на страницу аренды для повторного ввода данных
            return View("Rent", model);
        }
    }
}
