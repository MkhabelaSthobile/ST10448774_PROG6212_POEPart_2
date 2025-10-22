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

        /*[Required]
        [StringLength(100)]
        public string ModuleName { get; set; } = string.Empty;*/

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
        /*public DateTime? LastStatusUpdate { get; set; }
        public string? StatusUpdatedBy { get; set; }*/

        public decimal CalculateTotal(decimal hourlyRate)
        {
            return HoursWorked * hourlyRate;
        }

        public void SubmitForApproval()
        {
            Status = "Submitted";
            // Removed LastStatusUpdate for now
        }

        public void UpdateStatus(string newStatus)
        {
            Status = newStatus;
            // Removed LastStatusUpdate for now
        }
    }
}