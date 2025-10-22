using Microsoft.AspNetCore.Mvc;
using CMCS_App.Models;
using CMCS_App.Data;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace CMCS_App.Controllers
{
    public class LecturerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LecturerController> _logger;
        private readonly IWebHostEnvironment _environment;

        public LecturerController(ApplicationDbContext context, ILogger<LecturerController> logger, IWebHostEnvironment environment)
        {
            _context = context;
            _logger = logger;
            _environment = environment;
        }

        public IActionResult Index()
        {
            int currentLecturerId = 1;

            var claims = _context.Claims
                .Include(c => c.Lecturer)
                .Where(c => c.LecturerID == currentLecturerId)
                .OrderByDescending(c => c.SubmissionDate)
                .ToList();

            return View(claims);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitClaim(Claim claim, IFormFile supportingDocument)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    claim.LecturerID = 1;

                    // Get lecturer details to set module name and hourly rate
                    var lecturer = await _context.Lecturers.FindAsync(claim.LecturerID);
                    if (lecturer != null)
                    {
                        lecturer.ModuleName = lecturer.ModuleName;
                        claim.HourlyRate = lecturer.HourlyRate;
                    }

                    claim.TotalAmount = claim.CalculateTotal(lecturer.HourlyRate);
                    claim.Status = "Submitted";
                    claim.SubmissionDate = DateTime.Now;

                    // Handle file upload with validation
                    string uploadedFileName = await HandleFileUpload(supportingDocument);
                    if (!string.IsNullOrEmpty(uploadedFileName))
                    {
                        claim.SupportingDocument = uploadedFileName;
                    }

                    _context.Claims.Add(claim);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Claim submitted successfully!" +
                        (!string.IsNullOrEmpty(uploadedFileName) ? " Document uploaded successfully." : "");
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database error submitting claim");
                TempData["ErrorMessage"] = "Database error submitting claim. Please try again.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting claim");
                TempData["ErrorMessage"] = "Error submitting claim. Please try again.";
            }

            return await LoadIndexViewWithError();
        }

        private async Task<string> HandleFileUpload(IFormFile supportingDocument)
        {
            if (supportingDocument == null || supportingDocument.Length == 0)
                return null;

            try
            {
                // Validate file size (5MB limit)
                if (supportingDocument.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("SupportingDocument", "File size cannot exceed 5MB.");
                    return null;
                }

                // Validate file type
                var allowedExtensions = new[] { ".pdf", ".docx", ".xlsx", ".jpg", ".png", ".jpeg" };
                var fileExtension = Path.GetExtension(supportingDocument.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("SupportingDocument",
                        "Invalid file type. Allowed types: PDF, DOCX, XLSX, JPG, PNG.");
                    return null;
                }

                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");

                // Create directory if it doesn't exist
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var originalFileName = Path.GetFileNameWithoutExtension(supportingDocument.FileName);
                var uniqueFileName = $"{originalFileName}_{Guid.NewGuid():N}{fileExtension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await supportingDocument.CopyToAsync(fileStream);
                }

                _logger.LogInformation($"File uploaded successfully: {uniqueFileName}, Size: {supportingDocument.Length} bytes");
                return uniqueFileName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file: {FileName}", supportingDocument.FileName);
                ModelState.AddModelError("SupportingDocument", "Error uploading file. Please try again.");
                return null;
            }
        }

        private async Task<IActionResult> LoadIndexViewWithError()
        {
            int currentLecturerId = 1;
            var claims = await _context.Claims
                .Include(c => c.Lecturer)
                .Where(c => c.LecturerID == currentLecturerId)
                .OrderByDescending(c => c.SubmissionDate)
                .ToListAsync();

            return View("Index", claims);
        }

        public async Task<IActionResult> DownloadDocument(int claimId, string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return NotFound();
            }

            try
            {
                // Verify that the current user owns this claim
                int currentLecturerId = 1;
                var claim = await _context.Claims
                    .FirstOrDefaultAsync(c => c.ClaimID == claimId && c.LecturerID == currentLecturerId);

                if (claim == null || claim.SupportingDocument != fileName)
                {
                    return NotFound("File not found or access denied.");
                }

                var filePath = Path.Combine(_environment.WebRootPath, "uploads", fileName);

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound("File not found on server.");
                }

                var memory = new MemoryStream();
                using (var stream = new FileStream(filePath, FileMode.Open))
                {
                    await stream.CopyToAsync(memory);
                }
                memory.Position = 0;

                // Determine content type
                var contentType = GetContentType(fileName);
                var originalFileName = fileName.Contains('_') ?
                    fileName.Substring(0, fileName.LastIndexOf('_')) + Path.GetExtension(fileName) :
                    fileName;

                return File(memory, contentType, originalFileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file: {FileName}", fileName);
                TempData["ErrorMessage"] = "Error downloading file. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        private string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLower();
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream"
            };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteClaim(int id)
        {
            try
            {
                int currentLecturerId = 1;
                var claim = await _context.Claims
                    .FirstOrDefaultAsync(c => c.ClaimID == id && c.LecturerID == currentLecturerId);

                if (claim == null)
                {
                    return NotFound();
                }

                // Only allow deletion if claim is still pending or submitted
                if (claim.Status != "Pending" && claim.Status != "Submitted")
                {
                    TempData["ErrorMessage"] = "Cannot delete claim that has already been processed.";
                    return RedirectToAction(nameof(Index));
                }

                // Delete associated file if exists
                if (!string.IsNullOrEmpty(claim.SupportingDocument))
                {
                    var filePath = Path.Combine(_environment.WebRootPath, "uploads", claim.SupportingDocument);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.Claims.Remove(claim);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Claim deleted successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting claim: {ClaimId}", id);
                TempData["ErrorMessage"] = "Error deleting claim. Please try again.";
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var claim = await _context.Claims
                .Include(c => c.Lecturer)
                .FirstOrDefaultAsync(c => c.ClaimID == id);

            if (claim == null || claim.LecturerID != 1)
            {
                return NotFound();
            }

            return View(claim);
        }
    }
}