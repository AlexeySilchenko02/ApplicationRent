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

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            // Определяем доступные виды аренды на основе категории места
            bool isOnlineRentAvailable = place.Category == "Офис" || place.Category == "Фотостудия";

            // Создаем модель представления с данными пользователя и места, устанавливаем начальные даты
            var viewModel = new RequestsRentViewModel
            {
                PlaceId = place.Id,
                PlaceName = place.Name,
                UserName = user.FullNameUser,
                UserEmail = user.Email,
                UserPhone = user.PhoneNumber,
                StartRent = DateTime.Today, // Сегодняшняя дата для начала аренды
                EndRent = DateTime.Today.AddDays(1), // Завтрашняя дата для окончания аренды по умолчанию
                Category = place.Category, // Добавляем категорию
                IsOnlineRentAvailable = isOnlineRentAvailable // Добавляем доступность онлайн аренды
            };

            return View(viewModel);
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
                    // Не удалось получить пользователя(добавить обработку ошибок)
                    return Unauthorized();
                }

                // Создаем новую аренду с Email пользователя
                var rental = new Rental
                {
                    UserId = user.Id,
                    PlaceId = model.PlaceId,
                    StartRent = model.StartRent,
                    EndRent = model.EndRent,
                    UserEmail = user.Email
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestRent(RequestsRentViewModel model, int RentDuration)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }

                // Вычисляем конечную дату аренды, используя выбранный срок аренды
                var endRent = model.StartRent.AddMonths(RentDuration);

                var requestRent = new RequestsRent
                {
                    UserId = user.Id,
                    PlaceId = model.PlaceId,
                    StartRent = model.StartRent,
                    EndRent = endRent,
                    UserEmail = model.UserEmail ?? user.Email,
                    UserName = model.UserName ?? user.UserName,
                    UserPhone = model.UserPhone
                };

                _context.Add(requestRent);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            // В случае ошибки валидации, вернуть пользователя на форму
            return View("Rent", model);
        }
    }
    public class RequestsRentViewModel
    {
        public int PlaceId { get; set; }
        public string PlaceName { get; set; }
        public DateTime StartRent { get; set; }
        public DateTime EndRent { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string UserPhone { get; set; }
        public string Category { get; set; }
        public bool IsOnlineRentAvailable { get; set; }
    }
    public class RentViewModel
    {
        public int PlaceId { get; set; }
        public DateTime StartRent { get; set; }
        public DateTime EndRent { get; set; }
    }

}
