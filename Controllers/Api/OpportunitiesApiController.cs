using AcxiomCRM.Data;
using AcxiomCRM.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AcxiomCRM.Controllers.Api
{
    [ApiController]
    [Route("api/opportunities")]
    [Authorize]
    public class OpportunitiesApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        private static readonly string[] ValidStages =
        {
            "Prospecting",
            "Qualification",
            "Proposal",
            "Negotiation",
            "Closed"
        };

        private static readonly string[] ValidStatuses =
        {
            "Open",
            "Won",
            "Lost"
        };

        public OpportunitiesApiController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/opportunities
        [HttpGet]
        public async Task<IActionResult> GetOpportunities()
        {
            var opportunities = await _context.Opportunities
                .AsNoTracking()
                .Select(o => new
                {
                    o.Id,
                    o.Name,
                    o.Company,
                    o.Amount,
                    o.Probability,
                    o.Stage,
                    o.Status,
                    o.ExpectedCloseDate,
                    o.AssignedToUserId,
                    o.CustomerId,
                    o.LeadId,
                    o.CreatedDate
                })
                .ToListAsync();

            return Ok(opportunities);
        }

        // GET: api/opportunities/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOpportunity(int id)
        {
            var opportunity = await _context.Opportunities
                .AsNoTracking()
                .Where(o => o.Id == id)
                .Select(o => new
                {
                    o.Id,
                    o.Name,
                    o.Company,
                    o.Amount,
                    o.Probability,
                    o.Stage,
                    o.Status,
                    o.ExpectedCloseDate,
                    o.AssignedToUserId,
                    o.CustomerId,
                    o.LeadId,
                    o.CreatedDate
                })
                .FirstOrDefaultAsync();

            if (opportunity == null)
            {
                return NotFound(new
                {
                    message = "Opportunity not found."
                });
            }

            return Ok(opportunity);
        }

        // POST: api/opportunities
        [HttpPost]
        [Authorize(Roles = "Admin,Manager,Sales Executive")]
        public async Task<IActionResult> CreateOpportunity(
            [FromBody] OpportunityRequest request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            if (!ValidStages.Contains(request.Stage))
            {
                return BadRequest(new
                {
                    message = "Invalid opportunity stage."
                });
            }

            if (!ValidStatuses.Contains(request.Status))
            {
                return BadRequest(new
                {
                    message = "Invalid opportunity status."
                });
            }

            if (request.Amount <= 0)
            {
                return BadRequest(new
                {
                    message = "Opportunity amount must be greater than zero."
                });
            }

            if (request.Probability < 0 ||
                request.Probability > 100)
            {
                return BadRequest(new
                {
                    message = "Probability must be between 0 and 100."
                });
            }

            if (request.Status == "Open" &&
                request.ExpectedCloseDate.Date < DateTime.UtcNow.Date)
            {
                return BadRequest(new
                {
                    message = "Expected close date cannot be in the past for an open opportunity."
                });
            }

            var opportunity = new Opportunity
            {
                Name = request.Name.Trim(),
                Company = request.Company?.Trim(),
                Amount = request.Amount,
                Probability = request.Probability,
                Stage = request.Stage,
                Status = request.Status,
                ExpectedCloseDate = request.ExpectedCloseDate,
                CustomerId = request.CustomerId,
                LeadId = request.LeadId,
                CreatedDate = DateTime.UtcNow
            };

            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetOpportunity),
                new { id = opportunity.Id },
                new
                {
                    opportunity.Id,
                    opportunity.Name,
                    opportunity.Company,
                    opportunity.Amount,
                    opportunity.Probability,
                    opportunity.Stage,
                    opportunity.Status,
                    opportunity.ExpectedCloseDate
                });
        }

        // PUT: api/opportunities/5
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateOpportunity(
            int id,
            [FromBody] OpportunityRequest request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            if (!ValidStages.Contains(request.Stage))
            {
                return BadRequest(new
                {
                    message = "Invalid opportunity stage."
                });
            }

            if (!ValidStatuses.Contains(request.Status))
            {
                return BadRequest(new
                {
                    message = "Invalid opportunity status."
                });
            }

            if (request.Amount <= 0)
            {
                return BadRequest(new
                {
                    message = "Opportunity amount must be greater than zero."
                });
            }

            if (request.Probability < 0 ||
                request.Probability > 100)
            {
                return BadRequest(new
                {
                    message = "Probability must be between 0 and 100."
                });
            }

            if (request.Status == "Open" &&
                request.ExpectedCloseDate.Date < DateTime.UtcNow.Date)
            {
                return BadRequest(new
                {
                    message = "Expected close date cannot be in the past for an open opportunity."
                });
            }

            var opportunity =
                await _context.Opportunities.FindAsync(id);

            if (opportunity == null)
            {
                return NotFound(new
                {
                    message = "Opportunity not found."
                });
            }

            opportunity.Name = request.Name.Trim();
            opportunity.Company = request.Company?.Trim();
            opportunity.Amount = request.Amount;
            opportunity.Probability = request.Probability;
            opportunity.Stage = request.Stage;
            opportunity.Status = request.Status;
            opportunity.ExpectedCloseDate =
                request.ExpectedCloseDate;
            opportunity.CustomerId = request.CustomerId;
            opportunity.LeadId = request.LeadId;
            opportunity.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Opportunity updated successfully."
            });
        }

        // DELETE: api/opportunities/5
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteOpportunity(int id)
        {
            var opportunity =
                await _context.Opportunities.FindAsync(id);

            if (opportunity == null)
            {
                return NotFound(new
                {
                    message = "Opportunity not found."
                });
            }

            _context.Opportunities.Remove(opportunity);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    public class OpportunityRequest
    {
        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [StringLength(150)]
        public string? Company { get; set; }

        [Range(0.01, 1000000000)]
        public decimal Amount { get; set; }

        [Range(0, 100)]
        public int Probability { get; set; }

        [Required]
        [StringLength(50)]
        public string Stage { get; set; } = "Prospecting";

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Open";

        [Required]
        public DateTime ExpectedCloseDate { get; set; }

        public int? CustomerId { get; set; }

        public int? LeadId { get; set; }
    }
}