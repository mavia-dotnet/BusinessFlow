
using BusinessManagementSystem.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BusinessManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,Manager,Staff")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalCustomers = await _context.Customers
                .CountAsync(c => c.IsActive);

            ViewBag.TotalProducts = await _context.Products
                .CountAsync(p => p.IsActive);

            ViewBag.TotalServices = await _context.Services
                .CountAsync(s => s.IsActive);

            ViewBag.PendingOrders = await _context.Orders
                .CountAsync(o => o.Status == "Pending");

            ViewBag.PendingServiceRequests =
                await _context.ServiceRequests
                    .CountAsync(r => r.Status == "Pending");

            ViewBag.UnpaidInvoices = await _context.Invoices
                .CountAsync(i => i.Status == "Unpaid");

            return View();
        }
    }
}
