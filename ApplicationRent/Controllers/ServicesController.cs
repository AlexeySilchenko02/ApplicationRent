using Microsoft.AspNetCore.Mvc;

namespace ApplicationRent.Controllers
{
    public class ServicesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
