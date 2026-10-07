using AcxiomCRM.Data;
using AcxiomCRM.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Controllers
{
    [Authorize]
    public class LeadsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        private static readonly string[] ValidStatuses =
        {
            "New",
            "Contacted",
            "Qualified",
            "Converted",
            "Lost"
        };

        public LeadsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Leads
        public async Task<IActionResult> Index(
            string? search,
            string? status)
        {
            var query = _context.Leads.AsQueryable();

            // Sales Executives can only see their assigned leads
            if (User.IsInRole("Sales Executive"))
            {
                var currentUser =
                    await _userManager.GetUserAsync(User);

                query = query.Where(l =>
                    l.AssignedToUserId == currentUser!.Id);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(l =>
                    l.Name.Contains(search) ||
                    (l.Email != null &&
                     l.Email.Contains(search)) ||
                    (l.Phone != null &&
                     l.Phone.Contains(search)) ||
                    (l.Company != null &&
                     l.Company.Contains(search)));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(l =>
                    l.Status == status);
            }

            var leads = await query
                .OrderByDescending(l => l.CreatedDate)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.Statuses = ValidStatuses;

            return View(leads);
        }

        // GET: Leads/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var lead = await _context.Leads
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lead == null)
                return NotFound();

            // Sales Executive can only view assigned lead
            if (User.IsInRole("Sales Executive"))
            {
                var currentUser =
                    await _userManager.GetUserAsync(User);

                if (lead.AssignedToUserId != currentUser!.Id)
                    return Forbid();
            }

            return View(lead);
        }

        // GET: Leads/Create
        public IActionResult Create()
        {
            ViewBag.Statuses = ValidStatuses;

            return View();
        }

        // POST: Leads/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Lead lead)
        {
            if (!ValidStatuses.Contains(lead.Status))
            {
                ModelState.AddModelError(
                    "Status",
                    "Invalid lead status.");
            }

            if (lead.ExpectedValue <= 0)
            {
                ModelState.AddModelError(
                    "ExpectedValue",
                    "Expected value must be greater than zero.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Statuses = ValidStatuses;

                return View(lead);
            }

            var currentUser =
                await _userManager.GetUserAsync(User);

            // Automatically assign new lead to Sales Executive
            if (User.IsInRole("Sales Executive"))
            {
                lead.AssignedToUserId = currentUser!.Id;
            }

            lead.CreatedDate = DateTime.UtcNow;

            _context.Leads.Add(lead);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Lead created successfully.";

            return RedirectToAction(nameof(Index));
        }

        // GET: Leads/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var lead = await _context.Leads
                .FindAsync(id);

            if (lead == null)
                return NotFound();

            // Sales Executive can only edit assigned lead
            if (User.IsInRole("Sales Executive"))
            {
                var currentUser =
                    await _userManager.GetUserAsync(User);

                if (lead.AssignedToUserId != currentUser!.Id)
                    return Forbid();
            }

            ViewBag.Statuses = ValidStatuses;

            return View(lead);
        }

        // POST: Leads/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Lead lead)
        {
            if (id != lead.Id)
                return NotFound();

            var existingLead =
                await _context.Leads.FindAsync(id);

            if (existingLead == null)
                return NotFound();

            // Sales Executive can only edit assigned lead
            if (User.IsInRole("Sales Executive"))
            {
                var currentUser =
                    await _userManager.GetUserAsync(User);

                if (existingLead.AssignedToUserId != currentUser!.Id)
                    return Forbid();
            }

            if (!ValidStatuses.Contains(lead.Status))
            {
                ModelState.AddModelError(
                    "Status",
                    "Invalid lead status.");
            }

            if (lead.ExpectedValue <= 0)
            {
                ModelState.AddModelError(
                    "ExpectedValue",
                    "Expected value must be greater than zero.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Statuses = ValidStatuses;

                return View(lead);
            }

            existingLead.Name = lead.Name;
            existingLead.Email = lead.Email;
            existingLead.Phone = lead.Phone;
            existingLead.Company = lead.Company;
            existingLead.Status = lead.Status;
            existingLead.ExpectedValue = lead.ExpectedValue;
            existingLead.Source = lead.Source;
            existingLead.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Lead updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        // GET: Leads/Delete/5
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var lead = await _context.Leads
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lead == null)
                return NotFound();

            return View(lead);
        }

        // POST: Leads/Delete/5
        [Authorize(Roles = "Admin,Manager")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lead = await _context.Leads
                .FindAsync(id);

            if (lead == null)
                return NotFound();

            _context.Leads.Remove(lead);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Lead deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}