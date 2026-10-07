using AcxiomCRM.Data;
using AcxiomCRM.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Controllers
{
    [Authorize]
    public class OpportunitiesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

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

        public OpportunitiesController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Opportunities
        public async Task<IActionResult> Index(
            string? search,
            string? status,
            string? stage)
        {
            var query = _context.Opportunities.AsQueryable();

            // Sales Executives can only see assigned opportunities
            if (User.IsInRole("Sales Executive"))
            {
                var currentUser =
                    await _userManager.GetUserAsync(User);

                query = query.Where(o =>
                    o.AssignedToUserId == currentUser!.Id);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(o =>
                    o.Name.Contains(search) ||
                    (o.Company != null &&
                     o.Company.Contains(search)));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(o =>
                    o.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(stage))
            {
                query = query.Where(o =>
                    o.Stage == stage);
            }

            var opportunities = await query
                .OrderByDescending(o => o.CreatedDate)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.Stage = stage;
            ViewBag.Statuses = ValidStatuses;
            ViewBag.Stages = ValidStages;

            return View(opportunities);
        }

        // GET: Opportunities/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var opportunity =
                await _context.Opportunities
                    .FirstOrDefaultAsync(o => o.Id == id);

            if (opportunity == null)
                return NotFound();

            // Sales Executive can only view assigned opportunity
            if (User.IsInRole("Sales Executive"))
            {
                var currentUser =
                    await _userManager.GetUserAsync(User);

                if (opportunity.AssignedToUserId != currentUser!.Id)
                    return Forbid();
            }

            return View(opportunity);
        }

        // GET: Opportunities/Create
        public IActionResult Create()
        {
            ViewBag.Statuses = ValidStatuses;
            ViewBag.Stages = ValidStages;

            return View(new Opportunity
            {
                Status = "Open",
                Stage = "Prospecting",
                ExpectedCloseDate =
                    DateTime.Today.AddDays(30)
            });
        }

        // POST: Opportunities/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Opportunity opportunity)
        {
            ValidateOpportunity(opportunity);

            if (!ModelState.IsValid)
            {
                ViewBag.Statuses = ValidStatuses;
                ViewBag.Stages = ValidStages;

                return View(opportunity);
            }

            var currentUser =
                await _userManager.GetUserAsync(User);

            // Automatically assign new opportunity
            // to the Sales Executive who created it
            if (User.IsInRole("Sales Executive"))
            {
                opportunity.AssignedToUserId =
                    currentUser!.Id;
            }

            opportunity.CreatedDate = DateTime.UtcNow;

            _context.Opportunities.Add(opportunity);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Opportunity created successfully.";

            return RedirectToAction(nameof(Index));
        }

        // GET: Opportunities/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var opportunity =
                await _context.Opportunities.FindAsync(id);

            if (opportunity == null)
                return NotFound();

            // Sales Executive can only edit assigned opportunity
            if (User.IsInRole("Sales Executive"))
            {
                var currentUser =
                    await _userManager.GetUserAsync(User);

                if (opportunity.AssignedToUserId != currentUser!.Id)
                    return Forbid();
            }

            ViewBag.Statuses = ValidStatuses;
            ViewBag.Stages = ValidStages;

            return View(opportunity);
        }

        // POST: Opportunities/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Opportunity opportunity)
        {
            if (id != opportunity.Id)
                return NotFound();

            var existingOpportunity =
                await _context.Opportunities.FindAsync(id);

            if (existingOpportunity == null)
                return NotFound();

            // Sales Executive can only edit assigned opportunity
            if (User.IsInRole("Sales Executive"))
            {
                var currentUser =
                    await _userManager.GetUserAsync(User);

                if (existingOpportunity.AssignedToUserId !=
                    currentUser!.Id)
                {
                    return Forbid();
                }
            }

            ValidateOpportunity(opportunity);

            if (!ModelState.IsValid)
            {
                ViewBag.Statuses = ValidStatuses;
                ViewBag.Stages = ValidStages;

                return View(opportunity);
            }

            existingOpportunity.Name =
                opportunity.Name;

            existingOpportunity.Company =
                opportunity.Company;

            existingOpportunity.Amount =
                opportunity.Amount;

            existingOpportunity.Probability =
                opportunity.Probability;

            existingOpportunity.Stage =
                opportunity.Stage;

            existingOpportunity.Status =
                opportunity.Status;

            existingOpportunity.ExpectedCloseDate =
                opportunity.ExpectedCloseDate;

            existingOpportunity.CustomerId =
                opportunity.CustomerId;

            existingOpportunity.LeadId =
                opportunity.LeadId;

            existingOpportunity.UpdatedDate =
                DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Opportunity updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        // GET: Opportunities/Delete/5
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var opportunity =
                await _context.Opportunities
                    .FirstOrDefaultAsync(o => o.Id == id);

            if (opportunity == null)
                return NotFound();

            return View(opportunity);
        }

        // POST: Opportunities/Delete/5
        [Authorize(Roles = "Admin,Manager")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            int id)
        {
            var opportunity =
                await _context.Opportunities.FindAsync(id);

            if (opportunity == null)
                return NotFound();

            _context.Opportunities.Remove(opportunity);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Opportunity deleted successfully.";

            return RedirectToAction(nameof(Index));
        }

        private void ValidateOpportunity(
            Opportunity opportunity)
        {
            if (!ValidStages.Contains(opportunity.Stage))
            {
                ModelState.AddModelError(
                    "Stage",
                    "Invalid opportunity stage.");
            }

            if (!ValidStatuses.Contains(opportunity.Status))
            {
                ModelState.AddModelError(
                    "Status",
                    "Invalid opportunity status.");
            }

            if (opportunity.Status == "Open" &&
                opportunity.Amount <= 0)
            {
                ModelState.AddModelError(
                    "Amount",
                    "Amount must be greater than zero for an active opportunity.");
            }

            if (opportunity.Probability < 0 ||
                opportunity.Probability > 100)
            {
                ModelState.AddModelError(
                    "Probability",
                    "Probability must be between 0 and 100.");
            }

            if (opportunity.Status == "Open" &&
                opportunity.ExpectedCloseDate.Date < DateTime.Today)
            {
                ModelState.AddModelError(
                    "ExpectedCloseDate",
                    "Expected close date cannot be in the past for an active opportunity.");
            }
        }
    }
}