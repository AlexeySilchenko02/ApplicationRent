using Microsoft.AspNetCore.Mvc;

namespace ApplicationRent.Controllers
{
    public class FeedbackController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
