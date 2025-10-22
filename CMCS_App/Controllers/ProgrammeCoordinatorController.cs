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
        // Get claims from database with lecturer information
        var claims = _context.Claims
            .Include(c => c.Lecturer)
            .OrderByDescending(c => c.SubmissionDate)
            .ToList();

        return View(claims);
    }

    [HttpPost]
    public IActionResult Approve(int id)
    {
        try
        {
            var claim = _context.Claims
                .Include(c => c.Lecturer)
                .FirstOrDefault(c => c.ClaimID == id);

            if (claim != null)
            {
                claim.Status = "Approved by Coordinator";
                claim.RejectionReason = null; // Clear any previous rejection reason
                _context.SaveChanges();

                TempData["SuccessMessage"] = $"Claim #{claim.ClaimID} for {claim.Lecturer.FullName} has been approved.";
            }
            else
            {
                TempData["ErrorMessage"] = "Claim not found.";
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Error approving claim. Please try again.";
            Console.WriteLine($"Error approving claim: {ex.Message}");
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult Reject(int id, string rejectionReason)
    {
        try
        {
            var claim = _context.Claims
                .Include(c => c.Lecturer)
                .FirstOrDefault(c => c.ClaimID == id);

            if (claim != null)
            {
                if (string.IsNullOrEmpty(rejectionReason))
                {
                    TempData["ErrorMessage"] = "Please provide a reason for rejection.";
                    return RedirectToAction(nameof(Index));
                }

                claim.Status = "Rejected by Coordinator";
                claim.RejectionReason = rejectionReason;
                _context.SaveChanges();

                TempData["SuccessMessage"] = $"Claim #{claim.ClaimID} for {claim.Lecturer.FullName} has been rejected.";
            }
            else
            {
                TempData["ErrorMessage"] = "Claim not found.";
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Error rejecting claim. Please try again.";
            Console.WriteLine($"Error rejecting claim: {ex.Message}");
        }

        return RedirectToAction(nameof(Index));
    }

    // New action to view claim details
    public IActionResult Details(int id)
    {
        var claim = _context.Claims
            .Include(c => c.Lecturer)
            .FirstOrDefault(c => c.ClaimID == id);

        if (claim == null)
        {
            return NotFound();
        }

        return View(claim);
    }
}