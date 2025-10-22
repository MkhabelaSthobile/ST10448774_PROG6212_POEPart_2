using CMCS_App.Models;
using Microsoft.AspNetCore.Mvc;

namespace CMCS_App.Controllers
{
    public class AcademicManagerController : Controller
    {
        public IActionResult Index()
        {
            var claimsList = new List<Claim>
    {
        new Claim
        {
            ClaimID = 201,
            Lecturer = new Lecturer { LecturerID = 1, FullName = "Dr. Smith" },
            HoursWorked = 8,
            Month = "August",
            TotalAmount = 4000,
            Status = "Approved by Coordinator",
            SubmissionDate = DateTime.Now.AddDays(-3)
        },
        new Claim
        {
            ClaimID = 202,
            Lecturer = new Lecturer { LecturerID = 2, FullName = "Prof. Johnson" },
            HoursWorked = 10,
            Month = "September",
            TotalAmount = 6000,
            Status = "Approved by Manager",
            SubmissionDate = DateTime.Now.AddDays(-5)
        }
    };

            return View(claimsList);
        }
    }
}
