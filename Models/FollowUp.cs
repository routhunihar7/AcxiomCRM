using System.ComponentModel.DataAnnotations;

namespace AcxiomCRM.Models
{
    public class FollowUp
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        [Display(Name = "Follow-Up Title")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Follow-Up Date")]
        public DateTime FollowUpDate { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Planned";

        [StringLength(100)]
        public string? AssignedTo { get; set; }
        [Display(Name = "Assigned To User")]
public string? AssignedToUserId { get; set; }

        public int? CustomerId { get; set; }

        public int? LeadId { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedDate { get; set; }
    }
}