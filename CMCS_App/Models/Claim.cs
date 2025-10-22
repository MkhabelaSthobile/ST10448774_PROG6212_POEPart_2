using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMCS_App.Models
{
    public class Claim
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ClaimID { get; set; }

        [Required]
        public int LecturerID { get; set; }

        [Required]
        [StringLength(50)]
        public string Month { get; set; } = string.Empty;

        [Required]
        [Range(1, 200, ErrorMessage = "Hours worked must be between 1 and 200")]
        public int HoursWorked { get; set; }

        [Required]
        [Range(0.01, 1000, ErrorMessage = "Hourly rate must be between 0.01 and 1000")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal HourlyRate { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        [StringLength(100)]
        public string Status { get; set; } = "Pending";

        [Required]
        public DateTime SubmissionDate { get; set; } = DateTime.Now;

        public string? SupportingDocument { get; set; }

        public string? RejectionReason { get; set; }

        // Navigation property - can be null if not loaded
        public virtual Lecturer? Lecturer { get; set; }

        // Status tracking properties (for real-time updates)
        public DateTime? LastStatusUpdate { get; set; }
        public string? StatusUpdatedBy { get; set; }

        public decimal CalculateTotal()
        {
            TotalAmount = HoursWorked * HourlyRate;
            return TotalAmount;
        }

        public void SubmitForApproval()
        {
            Status = "Submitted";
            LastStatusUpdate = DateTime.Now;
            StatusUpdatedBy = "Lecturer";
        }

        public void UpdateStatus(string newStatus)
        {
            Status = newStatus;
            LastStatusUpdate = DateTime.Now;
        }

        public void UpdateStatus(string newStatus, string updatedBy)
        {
            Status = newStatus;
            LastStatusUpdate = DateTime.Now;
            StatusUpdatedBy = updatedBy;
        }

        // Status tracking methods
        public string GetStatusBadgeClass()
        {
            return Status switch
            {
                "Pending" or "Submitted" => "bg-warning",
                "Approved by Coordinator" => "bg-info",
                "Approved by Manager" => "bg-success",
                "Rejected" or "Rejected by Coordinator" or "Rejected by Manager" => "bg-danger",
                _ => "bg-secondary"
            };
        }

        public string GetStatusTimeline()
        {
            var timeline = $"Submitted: {SubmissionDate:dd MMM yyyy HH:mm}";

            if (LastStatusUpdate.HasValue)
            {
                timeline += $" | Last Update: {LastStatusUpdate.Value:dd MMM yyyy HH:mm}";
                if (!string.IsNullOrEmpty(StatusUpdatedBy))
                {
                    timeline += $" by {StatusUpdatedBy}";
                }
            }

            return timeline;
        }

        public bool IsPending()
        {
            return Status == "Pending" || Status == "Submitted";
        }

        public bool IsApproved()
        {
            return Status.Contains("Approved");
        }

        public bool IsRejected()
        {
            return Status.Contains("Rejected");
        }

        public TimeSpan GetProcessingTime()
        {
            return DateTime.Now - SubmissionDate;
        }

        public string GetProcessingTimeDisplay()
        {
            var processingTime = GetProcessingTime();

            if (processingTime.TotalDays >= 1)
                return $"{(int)processingTime.TotalDays} days";
            else if (processingTime.TotalHours >= 1)
                return $"{(int)processingTime.TotalHours} hours";
            else
                return $"{(int)processingTime.TotalMinutes} minutes";
        }
    }
}