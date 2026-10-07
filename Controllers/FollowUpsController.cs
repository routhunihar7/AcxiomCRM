using AcxiomCRM.Data;
using AcxiomCRM.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Controllers
{
    [Authorize]
    public class FollowUpsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        private static readonly string[] ValidStatuses =
        {
            "Planned",
            "Completed",
            "Cancelled"
        };

        public FollowUpsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: FollowUps
        public async Task<IActionResult> Index(
            string? search,
            string? status)
        {
            var query = _context.FollowUps.AsQueryable();

            // Sales Executive can only see assigned follow-ups
            if (User.IsInRole("Sales Executive"))
            {
                var currentUser =
                    await _userManager.GetUserAsync(User);

                query = query.Where(f =>
                    f.AssignedToUserId == currentUser!.Id);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(f =>
                    f.Title.Contains(search) ||
                    f.Description.Contains(search) ||
                    (f.AssignedTo != null &&
                     f.AssignedTo.Contains(search)));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(f =>
                    f.Status == status);
            }

            var followUps = await query
                .OrderBy(f => f.FollowUpDate)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.Statuses = ValidStatuses;

            return View(followUps);
        }

        // GET: FollowUps/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var followUp = await _context.FollowUps
                .FirstOrDefaultAsync(f => f.Id == id);

            if (followUp == null)
                return NotFound();

            // Sales Executive can only view assigned follow-up
            if (User.IsInRole("Sales Executive"))
            {
                var currentUser =
                    await _userManager.GetUserAsync(User);

                if (followUp.AssignedToUserId != currentUser!.Id)
                    return Forbid();
            }

            return View(followUp);
        }

        // GET: FollowUps/Create
        public IActionResult Create()
        {
            ViewBag.Statuses = ValidStatuses;

            return View(new FollowUp
            {
                FollowUpDate = DateTime.Today,
                Status = "Planned"
            });
        }

        // POST: FollowUps/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            FollowUp followUp)
        {
            if (!ValidStatuses.Contains(followUp.Status))
            {
                ModelState.AddModelError(
                    "Status",
                    "Invalid follow-up status.");
            }

            if (followUp.Status == "Planned" &&
                followUp.FollowUpDate.Date < DateTime.Today)
            {
                ModelState.AddModelError(
                    "FollowUpDate",
                    "Planned follow-up date cannot be before today.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Statuses = ValidStatuses;

                return View(followUp);
            }

            var currentUser =
                await _userManager.GetUserAsync(User);

            // Automatically assign new follow-up to Sales Executive
            if (User.IsInRole("Sales Executive"))
            {
                followUp.AssignedToUserId = currentUser!.Id;
            }

            followUp.CreatedDate = DateTime.UtcNow;

            _context.FollowUps.Add(followUp);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Follow-up created successfully.";

            return RedirectToAction(nameof(Index));
        }

        // GET: FollowUps/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var followUp =
                await _context.FollowUps.FindAsync(id);

            if (followUp == null)
                return NotFound();

            // Sales Executive can only edit assigned follow-up
            if (User.IsInRole("Sales Executive"))
            {
                var currentUser =
                    await _userManager.GetUserAsync(User);

                if (followUp.AssignedToUserId != currentUser!.Id)
                    return Forbid();
            }

            ViewBag.Statuses = ValidStatuses;

            return View(followUp);
        }

        // POST: FollowUps/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            FollowUp followUp)
        {
            if (id != followUp.Id)
                return NotFound();

            var existingFollowUp =
                await _context.FollowUps.FindAsync(id);

            if (existingFollowUp == null)
                return NotFound();

            // Sales Executive can only edit assigned follow-up
            if (User.IsInRole("Sales Executive"))
            {
                var currentUser =
                    await _userManager.GetUserAsync(User);

                if (existingFollowUp.AssignedToUserId != currentUser!.Id)
                    return Forbid();
            }

            if (!ValidStatuses.Contains(followUp.Status))
            {
                ModelState.AddModelError(
                    "Status",
                    "Invalid follow-up status.");
            }

            if (followUp.Status == "Planned" &&
                followUp.FollowUpDate.Date < DateTime.Today)
            {
                ModelState.AddModelError(
                    "FollowUpDate",
                    "Planned follow-up date cannot be before today.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Statuses = ValidStatuses;

                return View(followUp);
            }

            existingFollowUp.Title = followUp.Title;
            existingFollowUp.Description = followUp.Description;
            existingFollowUp.FollowUpDate =
                followUp.FollowUpDate;
            existingFollowUp.Status = followUp.Status;
            existingFollowUp.AssignedTo =
                followUp.AssignedTo;
            existingFollowUp.CustomerId =
                followUp.CustomerId;
            existingFollowUp.LeadId =
                followUp.LeadId;
            existingFollowUp.UpdatedDate =
                DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Follow-up updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        // GET: FollowUps/Delete/5
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var followUp = await _context.FollowUps
                .FirstOrDefaultAsync(f => f.Id == id);

            if (followUp == null)
                return NotFound();

            return View(followUp);
        }

        // POST: FollowUps/Delete/5
        [Authorize(Roles = "Admin,Manager")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            int id)
        {
            var followUp =
                await _context.FollowUps.FindAsync(id);

            if (followUp == null)
                return NotFound();

            _context.FollowUps.Remove(followUp);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Follow-up deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}