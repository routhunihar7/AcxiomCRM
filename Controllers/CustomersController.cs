using AcxiomCRM.Data;
using AcxiomCRM.Models;
using AcxiomCRM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Controllers
{
    [Authorize]
    public class CustomersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuditService _auditService;

        public CustomersController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            AuditService auditService)
        {
            _context = context;
            _userManager = userManager;
            _auditService = auditService;
        }

        // GET: Customers
        public async Task<IActionResult> Index(string? searchString)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            IQueryable<Customer> customers =
                _context.Customers.AsNoTracking();

            // Sales Executives can only see their assigned customers
            if (currentUser != null &&
                await _userManager.IsInRoleAsync(
                    currentUser,
                    "Sales Executive"))
            {
                customers = customers.Where(c =>
                    c.AssignedToUserId == currentUser.Id);
            }

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                searchString = searchString.Trim();

                customers = customers.Where(c =>
                    c.Name.Contains(searchString) ||
                    c.Email.Contains(searchString) ||
                    c.Phone.Contains(searchString) ||
                    (c.Company != null &&
                     c.Company.Contains(searchString)));
            }

            ViewData["CurrentFilter"] = searchString;

            return View(
                await customers
                    .OrderByDescending(c => c.CreatedDate)
                    .ToListAsync());
        }

        // GET: Customers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);

            // Sales Executive can only view assigned customers
            if (currentUser != null &&
                await _userManager.IsInRoleAsync(
                    currentUser,
                    "Sales Executive"))
            {
                if (customer.AssignedToUserId != currentUser.Id)
                {
                    return Forbid();
                }
            }

            return View(customer);
        }

        // GET: Customers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Customers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer)
        {
            if (!ModelState.IsValid)
            {
                return View(customer);
            }

            // Duplicate email validation
            var emailExists = await _context.Customers
                .AnyAsync(c =>
                    c.Email == customer.Email);

            if (emailExists)
            {
                ModelState.AddModelError(
                    "Email",
                    "A customer with this email already exists.");

                return View(customer);
            }

            // Duplicate phone validation
            var phoneExists = await _context.Customers
                .AnyAsync(c =>
                    c.Phone == customer.Phone);

            if (phoneExists)
            {
                ModelState.AddModelError(
                    "Phone",
                    "A customer with this phone already exists.");

                return View(customer);
            }

            var currentUser =
                await _userManager.GetUserAsync(User);

            // Automatically assign new customers
            // to the logged-in Sales Executive
            if (currentUser != null &&
                await _userManager.IsInRoleAsync(
                    currentUser,
                    "Sales Executive"))
            {
                customer.AssignedToUserId = currentUser.Id;
            }

            customer.CreatedDate = DateTime.UtcNow;

            _context.Customers.Add(customer);

            await _context.SaveChangesAsync();

            // AUDIT: Customer Created
            await _auditService.LogAsync(
                action: "Create",
                entityName: "Customer",
                recordId: customer.Id.ToString(),
                newValue: new
                {
                    customer.Name,
                    customer.Email,
                    customer.Phone,
                    customer.Company,
                    customer.Address,
                    customer.City,
                    customer.Country,
                    customer.AssignedToUserId
                });

            TempData["SuccessMessage"] =
                "Customer created successfully.";

            return RedirectToAction(nameof(Index));
        }

        // GET: Customers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null)
            {
                return NotFound();
            }

            var currentUser =
                await _userManager.GetUserAsync(User);

            // Sales Executive can only edit assigned customers
            if (currentUser != null &&
                await _userManager.IsInRoleAsync(
                    currentUser,
                    "Sales Executive"))
            {
                if (customer.AssignedToUserId != currentUser.Id)
                {
                    return Forbid();
                }
            }

            return View(customer);
        }

        // POST: Customers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Customer customer)
        {
            if (id != customer.Id)
            {
                return NotFound();
            }

            var existingCustomer =
                await _context.Customers
                    .FirstOrDefaultAsync(c => c.Id == id);

            if (existingCustomer == null)
            {
                return NotFound();
            }

            var currentUser =
                await _userManager.GetUserAsync(User);

            // Sales Executive can only edit assigned customers
            if (currentUser != null &&
                await _userManager.IsInRoleAsync(
                    currentUser,
                    "Sales Executive"))
            {
                if (existingCustomer.AssignedToUserId !=
                    currentUser.Id)
                {
                    return Forbid();
                }
            }

            if (!ModelState.IsValid)
            {
                return View(customer);
            }

            // Duplicate email validation
            var emailExists = await _context.Customers
                .AnyAsync(c =>
                    c.Id != id &&
                    c.Email == customer.Email);

            if (emailExists)
            {
                ModelState.AddModelError(
                    "Email",
                    "A customer with this email already exists.");

                return View(customer);
            }

            // Duplicate phone validation
            var phoneExists = await _context.Customers
                .AnyAsync(c =>
                    c.Id != id &&
                    c.Phone == customer.Phone);

            if (phoneExists)
            {
                ModelState.AddModelError(
                    "Phone",
                    "A customer with this phone already exists.");

                return View(customer);
            }

            // Capture old values BEFORE modifying
            var oldCustomer = new
            {
                existingCustomer.Id,
                existingCustomer.Name,
                existingCustomer.Email,
                existingCustomer.Phone,
                existingCustomer.Company,
                existingCustomer.Address,
                existingCustomer.City,
                existingCustomer.Country,
                existingCustomer.AssignedToUserId
            };

            // Update fields
            existingCustomer.Name = customer.Name;
            existingCustomer.Email = customer.Email;
            existingCustomer.Phone = customer.Phone;
            existingCustomer.Company = customer.Company;
            existingCustomer.Address = customer.Address;
            existingCustomer.City = customer.City;
            existingCustomer.Country = customer.Country;

            // Sales Executive cannot change assignment
            if (currentUser != null &&
                await _userManager.IsInRoleAsync(
                    currentUser,
                    "Sales Executive"))
            {
                existingCustomer.AssignedToUserId =
                    currentUser.Id;
            }
            else
            {
                existingCustomer.AssignedToUserId =
                    customer.AssignedToUserId;
            }

            existingCustomer.UpdatedDate =
                DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // AUDIT: Customer Updated
            await _auditService.LogAsync(
                action: "Update",
                entityName: "Customer",
                recordId: existingCustomer.Id.ToString(),
                oldValue: oldCustomer,
                newValue: new
                {
                    existingCustomer.Id,
                    existingCustomer.Name,
                    existingCustomer.Email,
                    existingCustomer.Phone,
                    existingCustomer.Company,
                    existingCustomer.Address,
                    existingCustomer.City,
                    existingCustomer.Country,
                    existingCustomer.AssignedToUserId
                });

            TempData["SuccessMessage"] =
                "Customer updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        // GET: Customers/Delete/5
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // POST: Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null)
            {
                return NotFound();
            }

            // Capture old values BEFORE deletion
            var oldCustomer = new
            {
                customer.Id,
                customer.Name,
                customer.Email,
                customer.Phone,
                customer.Company,
                customer.Address,
                customer.City,
                customer.Country,
                customer.AssignedToUserId
            };

            _context.Customers.Remove(customer);

            await _context.SaveChangesAsync();

            // AUDIT: Customer Deleted
            await _auditService.LogAsync(
                action: "Delete",
                entityName: "Customer",
                recordId: id.ToString(),
                oldValue: oldCustomer);

            TempData["SuccessMessage"] =
                "Customer deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}