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

                return Json(new { success = true, message = "Сообщение успешно отправлено!" });
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors.Select(b => b.ErrorMessage));
            return Json(new { success = false, message = errors });
        }
    }
}
