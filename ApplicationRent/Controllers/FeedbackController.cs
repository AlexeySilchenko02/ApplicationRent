using ApplicationRent.Data;
using ApplicationRent.Data.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ApplicationRent.Controllers
{
    public class FeedbackController : Controller
    {
        private readonly ApplicationDbContext _context;
        public FeedbackController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitFeedback(Feedback feedback)
        {
            if (ModelState.IsValid)
            {
                _context.Add(feedback);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Сообщение успешно отправлено!";
                return RedirectToAction(nameof(Index)); // Перезагружаем страницу, чтобы очистить форму и показать сообщение
            }

            return View("Index", feedback); // В случае ошибки валидации
        }
    }
}
