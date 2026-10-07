using System.ComponentModel.DataAnnotations;

namespace AcxiomCRM.Models
{
    public class Lead
    {
        [Display(Name = "Assigned To")]
public string? AssignedToUserId { get; set; }
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        [Display(Name = "Lead Name")]
        public string Name { get; set; } = string.Empty;

        [EmailAddress]
        [StringLength(150)]
        public string? Email { get; set; }

        [Phone]
        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(150)]
        public string? Company { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "New";

        [Range(0.01, 100000000)]
        [Display(Name = "Expected Value")]
        public decimal ExpectedValue { get; set; }

        [StringLength(100)]
        public string? Source { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedDate { get; set; }
    }
}