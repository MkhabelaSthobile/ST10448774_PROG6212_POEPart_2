namespace CMCS_App.Models
{
    public class Claim
    {
        public int ClaimID { get; set; }
        public int LecturerID { get; set; } // FK
        public Lecturer Lecturer { get; set; }   
        public string Month { get; set; } = string.Empty;
        public int HoursWorked { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime SubmissionDate { get; set; } = DateTime.Now;
        public string SupportingDocument { get; set; } = string.Empty;

        //The following properties are added simply for the prototype
        public string ModuleName { get; set; }
        public decimal HourlyRate { get; set; }
    

        // Methods
        public decimal CalculateTotal(decimal hourlyRate)
        {
            return HoursWorked * hourlyRate;
        }

        public void SubmitForApproval()
        {
            Status = "Submitted";
        }

        public void UpdateStatus(string newStatus)
        {
            Status = newStatus;
        }
    }
}
