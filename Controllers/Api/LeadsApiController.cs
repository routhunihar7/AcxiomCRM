using AcxiomCRM.Data;
using AcxiomCRM.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AcxiomCRM.Controllers.Api
{
    [ApiController]
    [Route("api/leads")]
    [Authorize]
    public class LeadsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        private static readonly string[] ValidStatuses =
        {
            "New",
            "Contacted",
            "Qualified",
            "Converted",
            "Lost"
        };

        public LeadsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/leads
        [HttpGet]
        public async Task<IActionResult> GetLeads()
        {
            var leads = await _context.Leads
                .AsNoTracking()
                .Select(l => new
                {
                    l.Id,
                    l.Name,
                    l.Email,
                    l.Phone,
                    l.Company,
                    l.Status,
                    l.ExpectedValue,
                    l.Source,
                    l.AssignedToUserId,
                    l.CreatedDate
                })
                .ToListAsync();

            return Ok(leads);
        }

        // GET: api/leads/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetLead(int id)
        {
            var lead = await _context.Leads
                .AsNoTracking()
                .Where(l => l.Id == id)
                .Select(l => new
                {
                    l.Id,
                    l.Name,
                    l.Email,
                    l.Phone,
                    l.Company,
                    l.Status,
                    l.ExpectedValue,
                    l.Source,
                    l.AssignedToUserId,
                    l.CreatedDate
                })
                .FirstOrDefaultAsync();

            if (lead == null)
            {
                return NotFound(new
                {
                    message = "Lead not found."
                });
            }

            return Ok(lead);
        }

        // POST: api/leads
        [HttpPost]
        [Authorize(Roles = "Admin,Manager,Sales Executive")]
        public async Task<IActionResult> CreateLead(
            [FromBody] LeadRequest request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            if (!ValidStatuses.Contains(request.Status))
            {
                return BadRequest(new
                {
                    message = "Invalid lead status."
                });
            }

            if (request.ExpectedValue <= 0)
            {
                return BadRequest(new
                {
                    message = "Expected value must be greater than zero."
                });
            }

            var lead = new Lead
            {
                Name = request.Name.Trim(),
                Email = request.Email?.Trim(),
                Phone = request.Phone?.Trim(),
                Company = request.Company?.Trim(),
                Status = request.Status,
                ExpectedValue = request.ExpectedValue,
                Source = request.Source?.Trim(),
                CreatedDate = DateTime.UtcNow
            };

            _context.Leads.Add(lead);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetLead),
                new { id = lead.Id },
                new
                {
                    lead.Id,
                    lead.Name,
                    lead.Email,
                    lead.Phone,
                    lead.Company,
                    lead.Status,
                    lead.ExpectedValue,
                    lead.Source
                });
        }

        // PUT: api/leads/5
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateLead(
            int id,
            [FromBody] LeadRequest request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            if (!ValidStatuses.Contains(request.Status))
            {
                return BadRequest(new
                {
                    message = "Invalid lead status."
                });
            }

            if (request.ExpectedValue <= 0)
            {
                return BadRequest(new
                {
                    message = "Expected value must be greater than zero."
                });
            }

            var lead = await _context.Leads.FindAsync(id);

            if (lead == null)
            {
                return NotFound(new
                {
                    message = "Lead not found."
                });
            }

            lead.Name = request.Name.Trim();
            lead.Email = request.Email?.Trim();
            lead.Phone = request.Phone?.Trim();
            lead.Company = request.Company?.Trim();
            lead.Status = request.Status;
            lead.ExpectedValue = request.ExpectedValue;
            lead.Source = request.Source?.Trim();
            lead.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Lead updated successfully."
            });
        }

        // DELETE: api/leads/5
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteLead(int id)
        {
            var lead = await _context.Leads.FindAsync(id);

            if (lead == null)
            {
                return NotFound(new
                {
                    message = "Lead not found."
                });
            }

            _context.Leads.Remove(lead);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    public class LeadRequest
    {
        [Required]
        [StringLength(150)]
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
        public decimal ExpectedValue { get; set; }

        [StringLength(100)]
        public string? Source { get; set; }
    }
}