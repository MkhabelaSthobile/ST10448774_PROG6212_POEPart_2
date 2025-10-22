using CMCS_App.Data;
using CMCS_App.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CMCS_App.Controllers
{
    public class AcademicManagerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AcademicManagerController> _logger;

        public AcademicManagerController(ApplicationDbContext context, ILogger<AcademicManagerController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            // Get claims that are approved by coordinator and ready for final verification
            var claims = _context.Claims
                .Include(c => c.Lecturer)
                .Where(c => c.Status == "Approved by Coordinator" || c.Status == "Approved by Manager" || c.Status.Contains("Rejected by Manager"))
                .OrderByDescending(c => c.SubmissionDate)
                .ToList();

            return View(claims);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Verify(int id)
        {
            try
            {
                var claim = _context.Claims
                    .Include(c => c.Lecturer)
                    .FirstOrDefault(c => c.ClaimID == id);

                if (claim != null)
                {
                    claim.Status = "Approved by Manager";
                    claim.RejectionReason = null; // Clear any previous rejection reason
                    _context.SaveChanges();

                    TempData["SuccessMessage"] = $"Claim #{claim.ClaimID} for {claim.Lecturer.FullName} has been verified and approved.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Claim not found.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying claim: {ClaimId}", id);
                TempData["ErrorMessage"] = "Error verifying claim. Please try again.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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

                    claim.Status = "Rejected by Manager";
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
                _logger.LogError(ex, "Error rejecting claim: {ClaimId}", id);
                TempData["ErrorMessage"] = "Error rejecting claim. Please try again.";
            }

            return RedirectToAction(nameof(Index));
        }

        // New action to generate reports
        public IActionResult GenerateReport(string reportType)
        {
            try
            {
                var claims = _context.Claims
                    .Include(c => c.Lecturer)
                    .ToList();

                var reportData = new
                {
                    TotalClaims = claims.Count,
                    ApprovedClaims = claims.Count(c => c.Status == "Approved by Manager"),
                    PendingClaims = claims.Count(c => c.Status == "Approved by Coordinator"),
                    RejectedClaims = claims.Count(c => c.Status.Contains("Rejected")),
                    TotalAmount = claims.Where(c => c.Status == "Approved by Manager").Sum(c => c.TotalAmount),
                    GeneratedDate = DateTime.Now
                };

                TempData["SuccessMessage"] = $"Report generated: {reportData.TotalClaims} total claims, {reportData.ApprovedClaims} approved, R{reportData.TotalAmount:N2} total amount.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating report");
                TempData["ErrorMessage"] = "Error generating report. Please try again.";
                return RedirectToAction(nameof(Index));
            }
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
}