﻿using Microsoft.AspNetCore.Authorization;
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

        //Получение списка пользователей
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        //Изменения статуса пользователя
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

        //Удаление пользователей
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

        //Получение списка обратной связи
        public async Task<IActionResult> FeedbackList()
        {
            var feedbacks = await _context.Feedbacks.ToListAsync();
            return View(feedbacks); 
        }

        //Изменение статуса обратной связи
        [HttpPost]
        public async Task<IActionResult> ChangeFeedbackStatus(int id)
        {
            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback != null)
            {
                feedback.Status = !feedback.Status; // Изменение статуса на противоположный
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("FeedbackList"); // Возвращаемся к списку обратной связи
        }

        //Удаление записи обратной связи
        [HttpPost]
        public async Task<IActionResult> DeleteFeedback(int id)
        {
            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback != null)
            {
                _context.Feedbacks.Remove(feedback);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("FeedbackList"); // Возвращаемся к списку обратной связи
        }

        // Возвращает представление со списком заявок на аренду
        public async Task<IActionResult> RentRequestsList()
        {
            var rentRequests = await _context.RequestsRents.ToListAsync();
            return View(rentRequests);
        }

        // Изменение статуса представление заявоки на аренду
        [HttpPost]
        public async Task<IActionResult> ChangeRequestStatus(int id)
        {
            var request = await _context.RequestsRents.FindAsync(id);
            if (request != null)
            {
                request.Status = !request.Status;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("RentRequestsList"); // Возвращение к списку заявок на аренду
        }

        //Удаление заявки на аренду
        [HttpPost]
        public async Task<IActionResult> DeleteRequest(int id)
        {
            var request = await _context.RequestsRents.FindAsync(id);
            if (request != null)
            {
                _context.RequestsRents.Remove(request);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("RentRequestsList"); // Возвращение к списку заявок на аренду
        }

        //Страница с просмотром онлайн аренд
        public async Task<IActionResult> Rentals()
        {
            var rentals = await _context.Rentals
                                .Include(r => r.User)
                                .Include(r => r.Place)
                                .OrderByDescending(r => r.Id)  // Сортировка по Id от большего к меньшему
                                .ToListAsync();
            return View(rentals);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteRental(int id)
        {
            var rental = await _context.Rentals.FindAsync(id);
            if (rental != null)
            {
                _context.Rentals.Remove(rental);
                await _context.SaveChangesAsync();
                return RedirectToAction("Rentals");
            }
            return NotFound();
        }
        /*
            МЕСТА АРЕНДЫ
         */

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
            //Категории
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
            //Категории
            ViewBag.Categories = new List<string> { "Фотостудия", "Склад", "Workspace", "Базовое место" };
            return View(place);
        }

        //Страница редактирования
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

        //Подтверждение удаления
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