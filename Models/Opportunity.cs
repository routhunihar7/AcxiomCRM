using System.ComponentModel.DataAnnotations;

namespace AcxiomCRM.Models
{
    public class Opportunity
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        [Display(Name = "Opportunity Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(150)]
        public string? Company { get; set; }

        [Range(0.01, 1000000000)]
        [Display(Name = "Amount")]
        public decimal Amount { get; set; }

        [Range(0, 100)]
        public int Probability { get; set; }

        [Required]
        [StringLength(50)]
        public string Stage { get; set; } = "Prospecting";

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Open";

        [Display(Name = "Expected Close Date")]
        public DateTime ExpectedCloseDate { get; set; }
       [Display(Name = "Assigned To")]
        public string? AssignedToUserId { get; set; }
        public int? CustomerId { get; set; }

        public int? LeadId { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedDate { get; set; }
    }
}