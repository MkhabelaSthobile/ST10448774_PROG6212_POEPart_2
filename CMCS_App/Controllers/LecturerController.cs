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

        public LecturerController(ApplicationDbContext context, ILogger<LecturerController> logger)
        {
            _context = context;
            _logger = logger;
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
                        claim.ModuleName = lecturer.ModuleName;
                        claim.HourlyRate = lecturer.HourlyRate;
                    }

                    // Calculate total amount
                    claim.TotalAmount = claim.CalculateTotal(claim.HourlyRate);
                    claim.Status = "Submitted";
                    claim.SubmissionDate = DateTime.Now;

                    // Handle file upload with validation
                    if (supportingDocument != null && supportingDocument.Length > 0)
                    {
                        // Validate file size (5MB limit)
                        if (supportingDocument.Length > 5 * 1024 * 1024)
                        {
                            ModelState.AddModelError("SupportingDocument", "File size cannot exceed 5MB.");
                            return await LoadIndexViewWithError();
                        }

                        // Validate file type
                        var allowedExtensions = new[] { ".pdf", ".docx", ".xlsx", ".jpg", ".png" };
                        var fileExtension = Path.GetExtension(supportingDocument.FileName).ToLower();

                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            ModelState.AddModelError("SupportingDocument",
                                "Invalid file type. Allowed types: PDF, DOCX, XLSX, JPG, PNG.");
                            return await LoadIndexViewWithError();
                        }

                        // For prototype, store in wwwroot/uploads (create this folder)
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

                        // Create directory if it doesn't exist
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        
                        var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await supportingDocument.CopyToAsync(fileStream);
                        }

                        claim.SupportingDocument = uniqueFileName; 

                        _logger.LogInformation($"File uploaded successfully: {uniqueFileName}, Original: {supportingDocument.FileName}");
                    }

                    
                    _context.Claims.Add(claim);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Claim submitted successfully!" +
                        (supportingDocument != null ? " Document uploaded successfully." : "");
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting claim");
                TempData["ErrorMessage"] = "Error submitting claim. Please try again.";
            }

            return await LoadIndexViewWithError();
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

        
        public IActionResult DownloadDocument(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return NotFound();
            }

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                stream.CopyTo(memory);
            }
            memory.Position = 0;

            // Determine content type
            var contentType = "application/octet-stream";
            var extension = Path.GetExtension(fileName).ToLower();
            switch (extension)
            {
                case ".pdf": contentType = "application/pdf"; break;
                case ".docx": contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document"; break;
                case ".xlsx": contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"; break;
                case ".jpg": case ".jpeg": contentType = "image/jpeg"; break;
                case ".png": contentType = "image/png"; break;
            }

            return File(memory, contentType, Path.GetFileName(filePath));
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