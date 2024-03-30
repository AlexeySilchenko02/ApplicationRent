using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ApplicationRent.Data.Identity;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ApplicationRent.Data;
using ApplicationRent.App_data;

namespace ApplicationRent.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationIdentityUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly FirebaseService _firebaseService;

        public AdminController(UserManager<ApplicationIdentityUser> userManager, ApplicationDbContext context, FirebaseService firebaseService)
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

        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        public async Task<IActionResult> ToggleAdminStatus(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                user.Admin = !user.Admin;
                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction("Users");
                }
            }
            return View("Error");
        }
        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                // Обработка случая, когда пользователь не найден
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Users));
            }
            else
            {
                // Обработка возможных ошибок при удалении
                return View("Error"); 
            }
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
    }
}