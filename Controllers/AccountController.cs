using Microsoft.AspNetCore.Mvc;
using StudentPortal.Data;
using StudentPortal.Models;

namespace StudentPortal.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var user = _context.Users
                .FirstOrDefault(x => x.Email == email && x.Password == password);

            if (user == null)
            {
                ViewBag.Error = "Invalid Login";
                return View();
            }

            HttpContext.Session.SetString("Role", user.Role);
            HttpContext.Session.SetInt32("UserId", user.UserId);

            if (user.Role == "Admin")
                return RedirectToAction("Index", "Students");

            return RedirectToAction("Profile", "Students");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
