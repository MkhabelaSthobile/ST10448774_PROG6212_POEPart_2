using System.Diagnostics;
using CMCS_App.Models;
using CMCS_App.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CMCS_App.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            // Check if user is already logged in
            if (HttpContext.Session.GetString("UserRole") != null)
            {
                return RedirectToDashboard();
            }
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            try
            {
                // Check lecturers
                var lecturer = _context.Lecturers.FirstOrDefault(l => l.Email == email && l.Password == password);
                if (lecturer != null)
                {
                    HttpContext.Session.SetString("UserRole", "Lecturer");
                    HttpContext.Session.SetString("UserName", lecturer.FullName);
                    HttpContext.Session.SetInt32("UserId", lecturer.LecturerID);
                    return RedirectToAction("Index", "Lecturer");
                }

                // Check programme coordinators
                var coordinator = _context.ProgrammeCoordinators.FirstOrDefault(pc => pc.Email == email);
                if (coordinator != null)
                {
                    HttpContext.Session.SetString("UserRole", "Coordinator");
                    HttpContext.Session.SetString("UserName", coordinator.Name);
                    HttpContext.Session.SetInt32("UserId", coordinator.CoordinatorID);
                    return RedirectToAction("Index", "ProgrammeCoordinator");
                }

                // Check academic managers
                var manager = _context.AcademicManagers.FirstOrDefault(am => am.Email == email);
                if (manager != null)
                {
                    HttpContext.Session.SetString("UserRole", "Manager");
                    HttpContext.Session.SetString("UserName", manager.FullName);
                    HttpContext.Session.SetInt32("UserId", manager.ManagerID);
                    return RedirectToAction("Index", "AcademicManager");
                }

                TempData["ErrorMessage"] = "Invalid email or password.";
                return View("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                TempData["ErrorMessage"] = "Error during login. Please try again.";
                return View("Index");
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "You have been logged out successfully.";
            return RedirectToAction("Index");
        }

        private IActionResult RedirectToDashboard()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            return userRole switch
            {
                "Lecturer" => RedirectToAction("Index", "Lecturer"),
                "Coordinator" => RedirectToAction("Index", "ProgrammeCoordinator"),
                "Manager" => RedirectToAction("Index", "AcademicManager"),
                _ => RedirectToAction("Index")
            };
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // New action for system status overview
        public IActionResult SystemStatus()
        {
            var status = new
            {
                TotalClaims = _context.Claims.Count(),
                PendingClaims = _context.Claims.Count(c => c.Status == "Submitted" || c.Status == "Pending"),
                ApprovedClaims = _context.Claims.Count(c => c.Status.Contains("Approved")),
                RejectedClaims = _context.Claims.Count(c => c.Status.Contains("Rejected")),
                TotalLecturers = _context.Lecturers.Count(),
                RecentSubmissions = _context.Claims
                    .OrderByDescending(c => c.SubmissionDate)
                    .Take(5)
                    .Select(c => new { c.ClaimID, c.Lecturer.FullName, c.Status, c.SubmissionDate })
                    .ToList()
            };

            return View(status);
        }
    }
}