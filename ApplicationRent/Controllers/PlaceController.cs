﻿using ApplicationRent.Data.Identity;
using ApplicationRent.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using ApplicationRent.App_data;
using ApplicationRent.Models;
using Microsoft.AspNetCore.Mvc.Rendering;


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

        /*public async Task<IActionResult> Index()
        {
            var places = await _context.Places.ToListAsync();

            // Передача статуса пользователя
            var user = await _userManager.GetUserAsync(User);
            var isAdmin = user?.Admin ?? false;
            ViewBag.IsAdmin = isAdmin;

            return View(places);
        }*/
        public async Task<IActionResult> Index(PlaceFilter filter)
        {
            var places = ApplyFilters(filter);

            var user = await _userManager.GetUserAsync(User);
            var isAdmin = user?.Admin ?? false;
            ViewBag.IsAdmin = isAdmin;

            return View(await places.ToListAsync());
        }

        public async Task<IActionResult> Filter(PlaceFilter filter)
        {
            var places = ApplyFilters(filter);
            return PartialView("_PlaceList", await places.ToListAsync());
        }

        private IQueryable<Place> ApplyFilters(PlaceFilter filter)
        {
            var places = _context.Places.AsQueryable();

            if (!string.IsNullOrEmpty(filter.Category))
            {
                places = places.Where(p => p.Category == filter.Category);
            }

            if (filter.InRent.HasValue)
            {
                places = places.Where(p => p.InRent == filter.InRent.Value);
            }

            if (!string.IsNullOrEmpty(filter.PriceSort))
            {
                places = filter.PriceSort == "PriceAsc" ? places.OrderBy(p => p.Price) : places.OrderByDescending(p => p.Price);
            }

            if (!string.IsNullOrEmpty(filter.SizeSort))
            {
                places = filter.SizeSort == "SizeAsc" ? places.OrderBy(p => p.SizePlace) : places.OrderByDescending(p => p.SizePlace);
            }

            return places;
        }

        public async Task<IActionResult> Calculator()
        {
            var places = await _context.Places.ToListAsync();
            ViewBag.Places = new SelectList(places, "Id", "Name");

            // Словарь для цен
            var prices = places.ToDictionary(p => p.Id, p => p.Price);
            ViewBag.Prices = prices;

            var model = new RentCalculatorViewModel(); // Инициализация модели
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Calculate(RentCalculatorViewModel model)
        {
            var place = await _context.Places.FindAsync(model.PlaceId);

            if (place != null)
            {
                var days = (model.EndDate - model.StartDate).Days + 1; // Включаем последний день
                var dailyRate = place.Price / 28;
                model.TotalCost = dailyRate * days;
            }

            return Json(new { totalCost = model.TotalCost.ToString("F2") });
        }

        // Фоновая проверка статуса арендованного места
        public async Task UpdateRentStatusAsync()
        {
            var places = await _context.Places.ToListAsync();
            var today = DateTime.Now;

            foreach (var place in places)
            {
                if (place.EndRent < today)
                {
                    place.InRent = false;
                }
                else
                {
                    place.InRent = true;
                }
                await _firebaseService.AddOrUpdatePlace(place);
            }
            await _context.SaveChangesAsync();
        }

        /*public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var place = await _context.Places
                .Include(p => p.Reviews)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (place == null)
            {
                return NotFound();
            }

            var averageRating = place.Reviews.Any() ? place.Reviews.Average(r => r.Rating) : 0;

            var model = new PlaceDetailsViewModel
            {
                Place = place,
                Reviews = place.Reviews.ToList(),
                NewReview = new ReviewViewModel { PlaceId = place.Id },
                AverageRating = averageRating
            };

            return View(model);
        }*/

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var place = await _context.Places
                .Include(p => p.Reviews)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (place == null)
            {
                return NotFound();
            }

            var averageRating = place.Reviews.Any() ? place.Reviews.Average(r => r.Rating) : 0;
            var userName = string.Empty;

            if (User.Identity.IsAuthenticated)
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

                if (user != null)
                {
                    userName = user.FullNameUser;
                }
            }

            var model = new PlaceDetailsViewModel
            {
                Place = place,
                Reviews = place.Reviews.ToList(),
                NewReview = new ReviewViewModel { PlaceId = place.Id, Name = userName },
                AverageRating = averageRating
            };

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> AddReview([FromBody] ReviewViewModel model)
        {
            if (model.Rating < 1 || model.Rating > 10)
            {
                return Json(new { success = false, message = "Поставьте оценку от 1 до 10" });
            }

            if (ModelState.IsValid)
            {
                var review = new Review
                {
                    PlaceId = model.PlaceId,
                    Name = model.Name,
                    Comment = model.Comment,
                    Rating = model.Rating
                };

                _context.Reviews.Add(review);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }

            return Json(new { success = false, message = "An error occurred while submitting your review." });
        }
        public async Task<IActionResult> GetAverageRating(int placeId)
        {
            var place = await _context.Places
                .Include(p => p.Reviews)
                .FirstOrDefaultAsync(m => m.Id == placeId);

            if (place == null)
            {
                return NotFound();
            }

            var averageRating = place.Reviews.Any() ? place.Reviews.Average(r => r.Rating) : 0;

            return Json(new { averageRating = averageRating });
        }

        //Подтверждение аренды
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
