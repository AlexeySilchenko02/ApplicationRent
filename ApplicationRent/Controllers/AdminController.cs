using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ApplicationRent.Data.Identity;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ApplicationRent.Data;
using ApplicationRent.App_data;
using Microsoft.Extensions.Hosting;
using System;
using ApplicationRent.Models;

namespace ApplicationRent.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationIdentityUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly FirebaseService _firebaseService;
        private readonly IWebHostEnvironment _hostEnvironment;

        public AdminController(UserManager<ApplicationIdentityUser> userManager, ApplicationDbContext context, FirebaseService firebaseService, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _firebaseService = firebaseService;
            _hostEnvironment = hostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var places = await _context.Places.ToListAsync();

            // Передача статуса пользователя
            var user = await _userManager.GetUserAsync(User);
            var isAdmin = user?.Admin ?? false;
            ViewBag.IsAdmin = isAdmin;

            return View(places);
        }

        /*public async Task<IActionResult> ViewReviews()
        {
            var places = await _context.Places.Include(p => p.Reviews).ToListAsync();
            return View(places);
        }

        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review != null)
            {
                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(ViewReviews));
        }*/

        public async Task<IActionResult> ViewReviews()
        {
            var places = await _context.Places.ToListAsync();
            return View(places);
        }

        public async Task<IActionResult> Reviews(int placeId)
        {
            var place = await _context.Places
                .Include(p => p.Reviews)
                .FirstOrDefaultAsync(p => p.Id == placeId);

            if (place == null)
            {
                return NotFound();
            }

            return View(place);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review != null)
            {
                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        //Получение списка пользователей
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        /*//Изменения статуса пользователя
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
        }*/

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

        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var model = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                FullNameUser = user.FullNameUser,
                Admin = user.Admin
            };

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null)
                {
                    return NotFound();
                }

                // Проверка на уникальность электронной почты
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null && existingUser.Id != model.Id)
                {
                    ModelState.AddModelError("Email", "Пользователь с такой электронной почтой уже существует.");
                    return View(model);
                }

                // Обновление свойств пользователя
                if (user.Email != model.Email)
                {
                    user.Email = model.Email;
                    user.UserName = model.Email;  // Обновление UserName
                }

                user.PhoneNumber = model.PhoneNumber;
                user.FullNameUser = model.FullNameUser;
                user.Admin = model.Admin;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction("Users");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }

        //Получение списка обратной связи
        public async Task<IActionResult> FeedbackList()
        {
            var feedbacks = await _context.Feedbacks.ToListAsync();
            return View(feedbacks); 
        }

        // Изменение статуса обратной связи
        [HttpPost]
        public async Task<IActionResult> ChangeFeedbackStatus(int id)
        {
            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback != null)
            {
                feedback.Status = !feedback.Status; // Изменение статуса на противоположный
                await _context.SaveChangesAsync();
                return Json(new { success = true, status = feedback.Status });
            }

            return Json(new { success = false });
        }

        // Удаление записи обратной связи
        [HttpPost]
        public async Task<IActionResult> DeleteFeedback(int id)
        {
            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback != null)
            {
                _context.Feedbacks.Remove(feedback);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }

            return Json(new { success = false });
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
                return Json(new { success = true, status = request.Status });
            }

            return Json(new { success = false });
        }

        // Удаление заявки на аренду
        [HttpPost]
        public async Task<IActionResult> DeleteRequest(int id)
        {
            var request = await _context.RequestsRents.FindAsync(id);
            if (request != null)
            {
                _context.RequestsRents.Remove(request);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }

            return Json(new { success = false });
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
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }
        public IActionResult ManagePhotos()
        {
            // Получите список файлов из папки "place"
            var photosDirectory = Path.Combine(_hostEnvironment.WebRootPath, "place");
            var photoFiles = Directory.GetFiles(photosDirectory)
                                       .Select(filePath => Path.GetFileName(filePath))
                                       .ToList();

            // Передайте список файлов в представление
            return View(photoFiles);
        }
        public IActionResult DownloadPhoto(string photoFileName)
        {
            var photosDirectory = Path.Combine(_hostEnvironment.WebRootPath, "place");
            var filePath = Path.Combine(photosDirectory, photoFileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                stream.CopyTo(memory);
            }
            memory.Position = 0;

            return File(memory, GetContentType(filePath), Path.GetFileName(filePath));
        }

        private string GetContentType(string path)
        {
            var types = GetMimeTypes();
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types[ext];
        }

        private Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
            {
                {".txt", "text/plain"},
                {".pdf", "application/pdf"},
                {".doc", "application/vnd.ms-word"},
                {".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"},
                {".xls", "application/vnd.ms-excel"},
                {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
                {".png", "image/png"},
                {".jpg", "image/jpeg"},
                {".jpeg", "image/jpeg"},
                {".gif", "image/gif"},
                {".csv", "text/csv"}
            };
        }

        [HttpPost]
        public IActionResult DeletePhoto(string photoFileName)
        {
            try
            {
                // Получение пути к файлу
                var filePath = Path.Combine(_hostEnvironment.WebRootPath, "place", photoFileName);

                // Проверка существования файла
                if (System.IO.File.Exists(filePath))
                {
                    // Удаление файла
                    System.IO.File.Delete(filePath);
                }

                return RedirectToAction("ManagePhotos");
            }
            catch (Exception ex)
            {
                // Обработка ошибки, если удаление не удалось
                return RedirectToAction("Error", "Home");
            }
        }

        /*
            МЕСТА АРЕНДЫ
         */

        //Вызов страницы создания записи
        public IActionResult Create()
        {
            ViewBag.Categories = new List<string> { "Фотостудия", "Склад", "Офис", "Базовое место" };
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Place place, IFormFile? imageFile1, IFormFile? imageFile2, IFormFile? imageFile3)
        {
            if (ModelState.IsValid)
            {
                // Сохранение изображений в папку "images" в корневом каталоге веб-сайта
                string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "place");

                if (imageFile1 != null)
                {
                    string uniqueFileName1 = Guid.NewGuid().ToString() + "_" + imageFile1.FileName;
                    string filePath1 = Path.Combine(uploadsFolder, uniqueFileName1);
                    var path = Path.Combine(_hostEnvironment.WebRootPath, "place", Path.GetFileName(imageFile1.FileName));


                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await imageFile1.CopyToAsync(stream);
                    }
                    place.ImageFileName1 = uniqueFileName1;
                }

                if (imageFile2 != null)
                {
                    string uniqueFileName2 = Guid.NewGuid().ToString() + "_" + imageFile2.FileName;
                    string filePath2 = Path.Combine(uploadsFolder, uniqueFileName2);
                    using (var fileStream = new FileStream(filePath2, FileMode.Create))
                    {
                        await imageFile2.CopyToAsync(fileStream);
                    }
                    place.ImageFileName2 = uniqueFileName2;
                }

                if (imageFile3 != null)
                {
                    string uniqueFileName3 = Guid.NewGuid().ToString() + "_" + imageFile3.FileName;
                    string filePath3 = Path.Combine(uploadsFolder, uniqueFileName3);
                    using (var fileStream = new FileStream(filePath3, FileMode.Create))
                    {
                        await imageFile3.CopyToAsync(fileStream);
                    }
                    place.ImageFileName3 = uniqueFileName3;
                }

                // Добавление информации о месте в базу данных
                _context.Add(place);
                await _context.SaveChangesAsync();

                // Сохранение в Firebase
                await _firebaseService.AddOrUpdatePlace(place);

                // Перенаправление на страницу со списком мест после успешного добавления
                return RedirectToAction(nameof(Index));
            }

            // Если модель не валидна, возвращаем пользователя обратно на форму с заполненными данными и ошибками валидации
            ViewBag.Categories = new List<string> { "Фотостудия", "Склад", "Офис", "Базовое место" };
            return View(place);
        }

        /*[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadPhoto(IFormFile photo)
        {
            if (photo != null && photo.Length > 0)
            {
                var path = Path.Combine(_hostEnvironment.WebRootPath, "place", Path.GetFileName(photo.FileName));
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await photo.CopyToAsync(stream);
                }
                return Json(new { success = true, message = "Фото успешно загружено!" });
            }

            return Json(new { success = false, message = "Ошибка при загрузке файла." });
        }*/

        //загрузка фото в файловую систему
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadPhoto(IFormFile photo)
        {
            if (photo != null && photo.Length > 0)
            {
                var fileName = Path.GetFileName(photo.FileName);
                var path = Path.Combine(_hostEnvironment.WebRootPath, "place", fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await photo.CopyToAsync(stream);
                }

                return Json(new { success = true, message = "Фото успешно загружено!", fileName = fileName });
            }

            return Json(new { success = false, message = "Ошибка при загрузке файла." });
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
            ViewBag.Categories = new List<string> { "Фотостудия", "Склад", "Офис", "Базовое место" };
            return View(place);
        }

        //Страница редактирования
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Place place, IFormFile? imageFile1, IFormFile? imageFile2, IFormFile? imageFile3)
        {
            if (id != place.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Сохранение изображений в папку "images" в корневом каталоге веб-сайта
                    string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "place");

                    if (imageFile1 != null)
                    {
                        string uniqueFileName1 = Guid.NewGuid().ToString() + "_" + imageFile1.FileName;
                        string filePath1 = Path.Combine(uploadsFolder, uniqueFileName1);
                        var path = Path.Combine(_hostEnvironment.WebRootPath, "place", Path.GetFileName(imageFile1.FileName));


                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await imageFile1.CopyToAsync(stream);
                        }
                        place.ImageFileName1 = uniqueFileName1;
                    }

                    if (imageFile2 != null)
                    {
                        string uniqueFileName2 = Guid.NewGuid().ToString() + "_" + imageFile2.FileName;
                        string filePath2 = Path.Combine(uploadsFolder, uniqueFileName2);
                        using (var fileStream = new FileStream(filePath2, FileMode.Create))
                        {
                            await imageFile2.CopyToAsync(fileStream);
                        }
                        place.ImageFileName1 = uniqueFileName2;
                    }

                    if (imageFile3 != null)
                    {
                        string uniqueFileName3 = Guid.NewGuid().ToString() + "_" + imageFile3.FileName;
                        string filePath3 = Path.Combine(uploadsFolder, uniqueFileName3);
                        using (var fileStream = new FileStream(filePath3, FileMode.Create))
                        {
                            await imageFile3.CopyToAsync(fileStream);
                        }
                        place.ImageFileName3 = uniqueFileName3;
                    }
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
            ViewBag.Categories = new List<string> { "Фотостудия", "Склад", "Офис", "Базовое место" };
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