using AcxiomCRM.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalCustomers =
                await _context.Customers.CountAsync();

            ViewBag.TotalLeads =
                await _context.Leads.CountAsync();

            ViewBag.OpenLeads =
                await _context.Leads.CountAsync(l =>
                    l.Status != "Converted" &&
                    l.Status != "Lost");

            ViewBag.TotalOpportunities =
                await _context.Opportunities.CountAsync();

            ViewBag.OpenOpportunities =
                await _context.Opportunities.CountAsync(o =>
                    o.Status == "Open");

            ViewBag.WonOpportunities =
                await _context.Opportunities.CountAsync(o =>
                    o.Status == "Won");

            ViewBag.LostOpportunities =
                await _context.Opportunities.CountAsync(o =>
                    o.Status == "Lost");

            ViewBag.PipelineValue =
                await _context.Opportunities
                    .Where(o => o.Status == "Open")
                    .SumAsync(o => (decimal?)o.Amount) ?? 0;

            ViewBag.LeadStatuses =
                await _context.Leads
                    .GroupBy(l => l.Status)
                    .Select(g => new
                    {
                        Status = g.Key,
                        Count = g.Count()
                    })
                    .OrderBy(x => x.Status)
                    .ToListAsync();

            ViewBag.OpportunityStatuses =
                await _context.Opportunities
                    .GroupBy(o => o.Status)
                    .Select(g => new
                    {
                        Status = g.Key,
                        Count = g.Count(),
                        Amount = g.Sum(o => o.Amount)
                    })
                    .OrderBy(x => x.Status)
                    .ToListAsync();

            return View();
        }
    }
}