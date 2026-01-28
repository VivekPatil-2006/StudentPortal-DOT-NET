using Microsoft.AspNetCore.Mvc;
using StudentPortal.Data;
using StudentPortal.Models;

namespace StudentPortal.Controllers
{
    public class NotificationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NotificationController(ApplicationDbContext context)
        {
            _context = context;
        }

        // SHOW ALL NOTIFICATIONS
        public IActionResult Index()
        {
            var notifications = _context.Notifications
                .OrderByDescending(n => n.CreatedAt)
                .ToList();

            return View(notifications);
        }

        // MARK AS READ
        public IActionResult MarkRead(int id)
        {
            var notification = _context.Notifications.Find(id);

            if (notification != null)
            {
                notification.IsRead = true;
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}
