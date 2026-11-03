using BusinessManagementSystem.Data;
using BusinessManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BusinessManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,Manager,Staff")]
    public class ServiceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ServiceController(ApplicationDbContext context)
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
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            ViewBag.Search = search;

            return View(serviceList);
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Service service)
        {
            if (!ModelState.IsValid)
            {
                return View(service);
            }

            service.CreatedAt = DateTime.UtcNow;
            service.IsActive = true;

            _context.Services.Add(service);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Service added successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var service = await _context.Services
                .FirstOrDefaultAsync(s =>
                    s.ServiceId == id &&
                    s.IsActive);

            if (service == null)
            {
                return NotFound();
            }

            return View(service);
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var service = await _context.Services
                .FindAsync(id);

            if (service == null || !service.IsActive)
            {
                return NotFound();
            }

            return View(service);
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Service service)
        {
            if (id != service.ServiceId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(service);
            }

            var existingService = await _context.Services
                .FindAsync(id);

            if (existingService == null ||
                !existingService.IsActive)
            {
                return NotFound();
            }

            existingService.Name = service.Name;
            existingService.Description = service.Description;
            existingService.Category = service.Category;
            existingService.Price = service.Price;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Service updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var service = await _context.Services
                .FindAsync(id);

            if (service == null)
            {
                return NotFound();
            }

            service.IsActive = false;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Service deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}