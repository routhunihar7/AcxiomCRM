using AcxiomCRM.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // ==========================================
            // KPI DATA
            // ==========================================

            var totalCustomers =
                await _context.Customers.CountAsync();

            var totalLeads =
                await _context.Leads.CountAsync();

            var openLeads =
                await _context.Leads.CountAsync(l =>
                    l.Status != "Converted" &&
                    l.Status != "Lost");

            var totalOpportunities =
                await _context.Opportunities.CountAsync();

            var openOpportunities =
                await _context.Opportunities.CountAsync(o =>
                    o.Status == "Open");

            var wonOpportunities =
                await _context.Opportunities.CountAsync(o =>
                    o.Status == "Won");

            var lostOpportunities =
                await _context.Opportunities.CountAsync(o =>
                    o.Status == "Lost");

            var pipelineValue =
                await _context.Opportunities
                    .Where(o => o.Status == "Open")
                    .SumAsync(o => (decimal?)o.Amount) ?? 0;


            // ==========================================
            // KPI VIEWBAG
            // ==========================================

            ViewBag.TotalCustomers =
                totalCustomers;

            ViewBag.TotalLeads =
                totalLeads;

            ViewBag.OpenLeads =
                openLeads;

            ViewBag.TotalOpportunities =
                totalOpportunities;

            ViewBag.OpenOpportunities =
                openOpportunities;

            ViewBag.WonOpportunities =
                wonOpportunities;

            ViewBag.LostOpportunities =
                lostOpportunities;

            ViewBag.PipelineValue =
                pipelineValue;


            // ==========================================
            // LEAD STATUS CHART
            // ==========================================

            ViewBag.NewLeads =
                await _context.Leads.CountAsync(l =>
                    l.Status == "New");

            ViewBag.ContactedLeads =
                await _context.Leads.CountAsync(l =>
                    l.Status == "Contacted");

            ViewBag.QualifiedLeads =
                await _context.Leads.CountAsync(l =>
                    l.Status == "Qualified");

            ViewBag.ConvertedLeads =
                await _context.Leads.CountAsync(l =>
                    l.Status == "Converted");

            ViewBag.LostLeads =
                await _context.Leads.CountAsync(l =>
                    l.Status == "Lost");


            // ==========================================
            // OPPORTUNITY PIPELINE CHART
            // ==========================================

            ViewBag.ProspectingOpportunities =
                await _context.Opportunities.CountAsync(o =>
                    o.Stage == "Prospecting");

            ViewBag.QualificationOpportunities =
                await _context.Opportunities.CountAsync(o =>
                    o.Stage == "Qualification");

            ViewBag.ProposalOpportunities =
                await _context.Opportunities.CountAsync(o =>
                    o.Stage == "Proposal");

            ViewBag.NegotiationOpportunities =
                await _context.Opportunities.CountAsync(o =>
                    o.Stage == "Negotiation");

            ViewBag.ClosedOpportunities =
                await _context.Opportunities.CountAsync(o =>
                    o.Stage == "Closed");


            // ==========================================
            // MONTHLY SALES
            // ==========================================

            var monthlySales =
                await _context.Opportunities
                    .Where(o => o.Status == "Won")
                    .GroupBy(o => new
                    {
                        o.CreatedDate.Year,
                        o.CreatedDate.Month
                    })
                    .Select(g => new
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        Total = g.Sum(o => o.Amount)
                    })
                    .OrderBy(x => x.Year)
                    .ThenBy(x => x.Month)
                    .ToListAsync();

            ViewBag.MonthlySalesLabels =
                monthlySales
                    .Select(x =>
                        $"{x.Year}-{x.Month:00}")
                    .ToList();

            ViewBag.MonthlySalesValues =
                monthlySales
                    .Select(x => x.Total)
                    .ToList();


            return View();
        }
    }
}