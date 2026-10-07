using AcxiomCRM.Data;
using AcxiomCRM.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Controllers.Api
{
    [ApiController]
    [Route("api/customers")]
    [Authorize]
    public class CustomersApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CustomersApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/customers
        [HttpGet]
        public async Task<IActionResult> GetCustomers()
        {
            var customers = await _context.Customers
                .AsNoTracking()
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Email,
                    c.Phone,
                    c.Company,
                    c.Address,
                    c.City,
                    c.Country,
                    c.AssignedToUserId,
                    c.CreatedDate
                })
                .ToListAsync();

            return Ok(customers);
        }

        // GET: api/customers/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCustomer(int id)
        {
            var customer = await _context.Customers
                .AsNoTracking()
                .Where(c => c.Id == id)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Email,
                    c.Phone,
                    c.Company,
                    c.Address,
                    c.City,
                    c.Country,
                    c.AssignedToUserId,
                    c.CreatedDate
                })
                .FirstOrDefaultAsync();

            if (customer == null)
            {
                return NotFound(new
                {
                    message = "Customer not found."
                });
            }

            return Ok(customer);
        }

        // POST: api/customers
        [HttpPost]
        [Authorize(Roles = "Admin,Manager,Sales Executive")]
        public async Task<IActionResult> CreateCustomer(
            [FromBody] CustomerRequest request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var emailExists = await _context.Customers
                .AnyAsync(c => c.Email == request.Email);

            if (emailExists)
            {
                return Conflict(new
                {
                    message = "A customer with this email already exists."
                });
            }

            var phoneExists = await _context.Customers
                .AnyAsync(c => c.Phone == request.Phone);

            if (phoneExists)
            {
                return Conflict(new
                {
                    message = "A customer with this phone already exists."
                });
            }

            var customer = new Customer
            {
                Name = request.Name.Trim(),
                Email = request.Email.Trim(),
                Phone = request.Phone.Trim(),
                Company = request.Company?.Trim(),
                Address = request.Address?.Trim(),
                City = request.City?.Trim(),
                Country = request.Country?.Trim(),
                CreatedDate = DateTime.UtcNow
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetCustomer),
                new { id = customer.Id },
                new
                {
                    customer.Id,
                    customer.Name,
                    customer.Email,
                    customer.Phone,
                    customer.Company,
                    customer.City,
                    customer.Country
                });
        }

        // PUT: api/customers/5
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateCustomer(
            int id,
            [FromBody] CustomerRequest request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var customer = await _context.Customers
                .FindAsync(id);

            if (customer == null)
            {
                return NotFound(new
                {
                    message = "Customer not found."
                });
            }

            var emailExists = await _context.Customers
                .AnyAsync(c =>
                    c.Id != id &&
                    c.Email == request.Email);

            if (emailExists)
            {
                return Conflict(new
                {
                    message = "A customer with this email already exists."
                });
            }

            var phoneExists = await _context.Customers
                .AnyAsync(c =>
                    c.Id != id &&
                    c.Phone == request.Phone);

            if (phoneExists)
            {
                return Conflict(new
                {
                    message = "A customer with this phone already exists."
                });
            }

            customer.Name = request.Name.Trim();
            customer.Email = request.Email.Trim();
            customer.Phone = request.Phone.Trim();
            customer.Company = request.Company?.Trim();
            customer.Address = request.Address?.Trim();
            customer.City = request.City?.Trim();
            customer.Country = request.Country?.Trim();
            customer.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Customer updated successfully."
            });
        }

        // DELETE: api/customers/5
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var customer = await _context.Customers
                .FindAsync(id);

            if (customer == null)
            {
                return NotFound(new
                {
                    message = "Customer not found."
                });
            }

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    public class CustomerRequest
    {
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.EmailAddress]
        [System.ComponentModel.DataAnnotations.StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.Phone]
        [System.ComponentModel.DataAnnotations.StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.StringLength(250)]
        public string? Company { get; set; }

        [System.ComponentModel.DataAnnotations.StringLength(500)]
        public string? Address { get; set; }

        [System.ComponentModel.DataAnnotations.StringLength(100)]
        public string? City { get; set; }

        [System.ComponentModel.DataAnnotations.StringLength(100)]
        public string? Country { get; set; }
    }
}