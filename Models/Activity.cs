using System.ComponentModel.DataAnnotations;

namespace AcxiomCRM.Models
{
    public class Activity
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string ActivityType { get; set; } = "Call";

        [StringLength(1000)]
        public string? Description { get; set; }

        public DateTime ActivityDate { get; set; } = DateTime.UtcNow;

        [StringLength(50)]
        public string Status { get; set; } = "Completed";

        [StringLength(450)]
        public string? UserId { get; set; }

        public int? CustomerId { get; set; }

        public int? LeadId { get; set; }

        public int? OpportunityId { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}