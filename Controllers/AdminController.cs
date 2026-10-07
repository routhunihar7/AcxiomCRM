using AcxiomCRM.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
 
        private static readonly string[] ValidRoles =
        {
            "Admin",
            "Manager",
            "Sales Executive"
        };

        public AdminController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users
                .OrderBy(u => u.FullName)
                .ToListAsync();

            var userRoles =
                new Dictionary<string, IList<string>>();

            foreach (var user in users)
            {
                userRoles[user.Id] =
                    await _userManager.GetRolesAsync(user);
            }

            ViewBag.UserRoles = userRoles;

            return View(users);
        }

        public async Task<IActionResult> EditRole(string? id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound();

            var currentRoles =
                await _userManager.GetRolesAsync(user);

            ViewBag.Roles = ValidRoles;
            ViewBag.CurrentRole =
                currentRoles.FirstOrDefault();

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRole(
            string id,
            string role)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            if (!ValidRoles.Contains(role))
            {
                TempData["ErrorMessage"] =
                    "Invalid role selected.";

                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound();

            var currentRoles =
                await _userManager.GetRolesAsync(user);

            if (currentRoles.Any())
            {
                var removeResult =
                    await _userManager.RemoveFromRolesAsync(
                        user,
                        currentRoles);

                if (!removeResult.Succeeded)
                {
                    TempData["ErrorMessage"] =
                        "Unable to remove existing role.";

                    return RedirectToAction(nameof(Index));
                }
            }

            if (!await _roleManager.RoleExistsAsync(role))
            {
                TempData["ErrorMessage"] =
                    "Selected role does not exist.";

                return RedirectToAction(nameof(Index));
            }

            var addResult =
                await _userManager.AddToRoleAsync(
                    user,
                    role);

            if (!addResult.Succeeded)
            {
                TempData["ErrorMessage"] =
                    "Unable to assign the selected role.";

                return RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] =
                $"Role updated successfully for {user.FullName}.";

            return RedirectToAction(nameof(Index));
        }
              public IActionResult CreateUser()
        {
            ViewBag.Roles = ValidRoles;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(
            string fullName,
            string email,
            string password,
            string role)
        {
            ViewBag.Roles = ValidRoles;

            if (string.IsNullOrWhiteSpace(fullName))
            {
                ModelState.AddModelError(
                    "FullName",
                    "Full name is required.");
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError(
                    "Email",
                    "Email is required.");
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(
                    "Password",
                    "Password is required.");
            }

            if (!ValidRoles.Contains(role))
            {
                ModelState.AddModelError(
                    "Role",
                    "Invalid role selected.");
            }

            if (!ModelState.IsValid)
            {
                return View();
            }

            var existingUser =
                await _userManager.FindByEmailAsync(email);

            if (existingUser != null)
            {
                ModelState.AddModelError(
                    "Email",
                    "A user with this email already exists.");

                return View();
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FullName = fullName,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            var result =
                await _userManager.CreateAsync(
                    user,
                    password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                return View();
            }

            var roleResult =
                await _userManager.AddToRoleAsync(
                    user,
                    role);

            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);

                foreach (var error in roleResult.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                return View();
            }

            TempData["SuccessMessage"] =
                $"User {fullName} created successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}