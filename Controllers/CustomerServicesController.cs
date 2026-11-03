using BusinessManagementSystem.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BusinessManagementSystem.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CustomerServicesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerServicesController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? search)
        {
            var services = _context.Services
                .Where(s => s.IsActive);

            if (!string.IsNullOrWhiteSpace(search))
            {
                services = services.Where(s =>
                    s.Name.Contains(search) ||
                    (s.Category != null &&
                     s.Category.Contains(search)));
            }

            var serviceList = await services
                .OrderBy(s => s.Name)
                .ToListAsync();

            ViewBag.Search = search;

            return View(serviceList);
        }
    }
}