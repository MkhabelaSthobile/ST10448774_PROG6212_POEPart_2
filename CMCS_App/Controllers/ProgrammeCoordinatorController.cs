using CMCS_App.Data;
using CMCS_App.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

public class ProgrammeCoordinatorController : Controller
{
    private readonly ApplicationDbContext _context;

    public ProgrammeCoordinatorController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
{
    var claimsList = new List<Claim>
    {           
        new Claim
        {
            ClaimID = 101,
            LecturerID = 1,
            HoursWorked = 12,
            Month = "August",
            Status = "Submitted",
            SubmissionDate = DateTime.Now,
            ModuleName = "Computer Science 101",
            HourlyRate = 500m
        },
        new Claim
        {
            ClaimID = 102,
            LecturerID = 2,
            HoursWorked = 10,
            Month = "August",
            Status = "Approved by Coordinator",
            SubmissionDate = DateTime.Now.AddDays(-2),
            ModuleName = "Software Engineering",
            HourlyRate = 600m
        },
        new Claim
        {
            ClaimID = 103,
            LecturerID = 3,
            HoursWorked = 15,
            Month = "September",
            Status = "Rejected by Coordinator",
            SubmissionDate = DateTime.Now.AddDays(-1),
            ModuleName = "Data Structures",
            HourlyRate = 550m
        }
    };

    return View(claimsList);
}


    [HttpPost]
    public IActionResult Approve(int id)
    {
        var claim = _context.Claims.Find(id);
        if (claim != null)
        {
            claim.Status = "Approved by Coordinator";
            _context.SaveChanges();
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult Reject(int id)
    {
        var claim = _context.Claims.Find(id);
        if (claim != null)
        {
            claim.Status = "Rejected by Coordinator";
            _context.SaveChanges();
        }
        return RedirectToAction(nameof(Index));
    }
}
