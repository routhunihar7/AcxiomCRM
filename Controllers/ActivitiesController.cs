using AcxiomCRM.Data;
using AcxiomCRM.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Controllers
{
    [Authorize]
    public class ActivitiesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ActivitiesController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var activities = await _context.Activities
                .AsNoTracking()
                .OrderByDescending(a => a.ActivityDate)
                .ToListAsync();

            return View(activities);
        }

        public IActionResult Create()
        {
            return View(new Activity
            {
                ActivityDate = DateTime.Now,
                ActivityType = "Call",
                Status = "Completed"
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Activity activity)
        {
            if (!ModelState.IsValid)
            {
                return View(activity);
            }

            var user = await _userManager.GetUserAsync(User);

            activity.UserId = user?.Id;
            activity.CreatedDate = DateTime.UtcNow;

            _context.Activities.Add(activity);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Activity created successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Delete(int id)
        {
            var activity = await _context.Activities.FindAsync(id);

            if (activity == null)
            {
                return NotFound();
            }

            _context.Activities.Remove(activity);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Activity deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}