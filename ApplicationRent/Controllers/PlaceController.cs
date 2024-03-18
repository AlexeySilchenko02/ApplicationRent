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

        //Вызов страницы создания записи
        public IActionResult Create()
        {
            ViewBag.Categories = new List<string> { "Фотостудия", "Склад", "Workspace", "Базовое место" };
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Place place)
        {
            if (ModelState.IsValid)
            {
                _context.Add(place);
                await _context.SaveChangesAsync();

                // Сохранение в Firebase
                await _firebaseService.AddOrUpdatePlace(place);

                return RedirectToAction(nameof(Index));
            }
            // Предполагаемые категории
            ViewBag.Categories = new List<string> { "Фотостудия", "Склад", "Workspace", "Базовое место" };
            return View(place);
        }

        //Вызов страницы редактирования
        public async Task<IActionResult> Edit(int? id)
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
            // Предполагаемые категории
            ViewBag.Categories = new List<string> { "Фотостудия", "Склад", "Workspace", "Базовое место" };
            return View(place);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Place place)
        {
            if (id != place.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(place);
                    await _context.SaveChangesAsync();

                    // Обновление в Firebase
                    await _firebaseService.AddOrUpdatePlace(place);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PlaceExists(place.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            // Предполагаемые категории (передача снова в случае ошибки валидации)
            ViewBag.Categories = new List<string> { "Фотостудия", "Склад", "Workspace", "Базовое место" };
            return View(place);
        }

        //Вызов страницы удаления
        public async Task<IActionResult> Delete(int? id)
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

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var place = await _context.Places.FindAsync(id);
            _context.Places.Remove(place);
            await _context.SaveChangesAsync();

            // Удаление из Firebase
            await _firebaseService.DeletePlace(id);

            return RedirectToAction(nameof(Index));
        }

        private bool PlaceExists(int id)
        {
            return _context.Places.Any(e => e.Id == id);
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
                // Создаем новую аренду
                var rental = new Rental
                {
                    UserId = _userManager.GetUserId(User),
                    PlaceId = model.PlaceId,
                    StartRent = model.StartRent,
                    EndRent = model.EndRent
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

                // Обновление в Firebase
                await _firebaseService.AddOrUpdatePlace(place);

                return RedirectToAction(nameof(Index));
            }

            // В случае ошибки возвращаем пользователя на страницу аренды для повторного ввода данных
            return View("Rent", model);
        }
    }
    public class RentViewModel
    {
        public int PlaceId { get; set; }
        public DateTime StartRent { get; set; }
        public DateTime EndRent { get; set; }
    }
}
