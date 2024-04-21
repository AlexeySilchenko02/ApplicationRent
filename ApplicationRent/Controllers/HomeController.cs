using ApplicationRent.Data;
using ApplicationRent.Data.Identity;
using ApplicationRent.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ApplicationRent.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        //�������� ����� �������� �����
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitFeedback(Feedback feedback)
        {
            if (ModelState.IsValid)
            {
                _context.Add(feedback);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "��������� ������� ����������!";
                return RedirectToAction(nameof(Index)); // ������������� ��������, ����� �������� ����� � �������� ���������
            }

            return View("Index", feedback); // � ������ ������ ���������
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}