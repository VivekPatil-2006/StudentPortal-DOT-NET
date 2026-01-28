using Microsoft.AspNetCore.Mvc;
using StudentPortal.Data;
using StudentPortal.Models;
using Rotativa.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace StudentPortal.Controllers
{
    public class StudentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public StudentsController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // =========================
        // ADMIN DASHBOARD (LIST)
        // =========================
        public IActionResult Index(int page = 1, int pageSize = 5, string course = "", string gender = "")
        {
            var query = _context.Students.AsQueryable();

            if (!string.IsNullOrEmpty(course))
                query = query.Where(s => s.Course == course);

            if (!string.IsNullOrEmpty(gender))
                query = query.Where(s => s.Gender == gender);

            int totalStudents = query.Count();

            var students = query
                .OrderByDescending(s => s.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Pagination
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalStudents / pageSize);

            // Dashboard
            ViewBag.TotalStudents = _context.Students.Count();
            ViewBag.TodayStudents = _context.Students.Count(s => s.CreatedAt.Date == DateTime.Today);

            ViewBag.RecentStudents = _context.Students
                .OrderByDescending(s => s.CreatedAt)
                .Take(5)
                .ToList();

            // Filters
            ViewBag.SelectedCourse = course;
            ViewBag.SelectedGender = gender;

            ViewBag.Courses = _context.Students
                .Select(s => s.Course)
                .Distinct()
                .ToList();

            return View(students);
        }

        // =========================
        // CREATE STUDENT
        // =========================
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Student student)
        {
            if (!ModelState.IsValid)
                return View(student);

            // PHOTO UPLOAD
            if (student.Photo != null)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                string fileName = Guid.NewGuid() + Path.GetExtension(student.Photo.FileName);
                string filePath = Path.Combine(uploadsFolder, fileName);

                using var fileStream = new FileStream(filePath, FileMode.Create);
                student.Photo.CopyTo(fileStream);

                student.PhotoPath = "/uploads/" + fileName;
            }

            student.CreatedAt = DateTime.Now;

            _context.Students.Add(student);
            _context.SaveChanges(); // StudentId generated

            // SAVE PARENT DETAILS
            var parent = new StudentParent
            {
                StudentId = student.StudentId,
                FatherName = Request.Form["FatherName"],
                FatherMobile = Request.Form["FatherMobile"],
                MotherName = Request.Form["MotherName"],
                MotherMobile = Request.Form["MotherMobile"],
                Address = Request.Form["Address"]
            };

            _context.StudentParents.Add(parent);

            // NOTIFICATION FOR ADMIN
            _context.Notifications.Add(new Notification
            {
                Message = "New student added: " + student.FullName
            });

            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // EDIT STUDENT
        // =========================
        public IActionResult Edit(int id)
        {
            var student = _context.Students.Find(id);
            if (student == null)
                return NotFound();

            return View(student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Student student)
        {
            if (!ModelState.IsValid)
                return View(student);

            var dbStudent = _context.Students.FirstOrDefault(x => x.StudentId == student.StudentId);

            if (dbStudent == null)
                return NotFound();

            // PHOTO UPDATE
            if (student.Photo != null)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                string fileName = Guid.NewGuid() + Path.GetExtension(student.Photo.FileName);
                string filePath = Path.Combine(uploadsFolder, fileName);

                using var fileStream = new FileStream(filePath, FileMode.Create);
                student.Photo.CopyTo(fileStream);

                dbStudent.PhotoPath = "/uploads/" + fileName;
            }

            // UPDATE FIELDS
            dbStudent.FullName = student.FullName;
            dbStudent.Email = student.Email;
            dbStudent.Mobile = student.Mobile;
            dbStudent.Gender = student.Gender;
            dbStudent.Course = student.Course;
            dbStudent.DateOfBirth = student.DateOfBirth;

            // NOTIFICATION
            _context.Notifications.Add(new Notification
            {
                Message = "Student profile updated: " + dbStudent.FullName
            });

            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // DELETE STUDENT
        // =========================
        public IActionResult Delete(int id)
        {
            var student = _context.Students.Find(id);

            if (student == null)
                return NotFound();

            _context.Students.Remove(student);

            _context.Notifications.Add(new Notification
            {
                Message = "Student deleted: " + student.FullName
            });

            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // EXPORT CSV
        // =========================
        public IActionResult ExportCSV()
        {
            var students = _context.Students.ToList();

            var csv = "FullName,Email,Mobile,Gender,Course,DateOfBirth,PhotoPath\n";

            foreach (var s in students)
            {
                csv += $"{s.FullName},{s.Email},{s.Mobile},{s.Gender},{s.Course}," +
                       $"{s.DateOfBirth?.ToString("yyyy-MM-dd")},{s.PhotoPath}\n";
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
            return File(bytes, "text/csv", "Students_Format.csv");
        }

        // =========================
        // EXPORT PDF
        // =========================
        public IActionResult ExportPDF()
        {
            var students = _context.Students
                .OrderByDescending(s => s.CreatedAt)
                .ToList();

            return new ViewAsPdf("StudentPdf", students)
            {
                FileName = "Students_Report.pdf"
            };
        }

        // =========================
        // IMPORT CSV
        // =========================
        [HttpPost]
        public IActionResult ImportCSV(IFormFile csvFile)
        {
            if (csvFile == null || csvFile.Length == 0)
                return RedirectToAction(nameof(Index));

            using var reader = new StreamReader(csvFile.OpenReadStream());
            bool isHeader = true;

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();

                if (isHeader)
                {
                    isHeader = false;
                    continue;
                }

                var values = line.Split(',');

                var student = new Student
                {
                    FullName = values[0],
                    Email = values[1],
                    Mobile = values[2],
                    Gender = values[3],
                    Course = values[4],
                    DateOfBirth = string.IsNullOrEmpty(values[5]) ? null : DateTime.Parse(values[5]),
                    PhotoPath = values.Length > 6 ? values[6] : null,
                    CreatedAt = DateTime.Now
                };

                _context.Students.Add(student);
            }

            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // DETAILS VIEW
        // =========================
        public IActionResult Details(int id)
        {
            var student = _context.Students
                .Where(s => s.StudentId == id)
                .Select(s => new
                {
                    Student = s,
                    Parent = _context.StudentParents.FirstOrDefault(p => p.StudentId == id)
                })
                .FirstOrDefault();

            return View(student);
        }

        // =========================
        // STUDENT PROFILE (LOGIN USER)
        // =========================
        public IActionResult Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            var user = _context.Users.Find(userId);

            var student = _context.Students
                .FirstOrDefault(x => x.StudentId == user.StudentId);

            return View(student);
        }
    }
}
