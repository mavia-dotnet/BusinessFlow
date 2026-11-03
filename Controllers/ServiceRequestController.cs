using BusinessManagementSystem.Data;
using BusinessManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BusinessManagementSystem.Controllers
{
    [Authorize(Roles = "Customer")]
    public class ServiceRequestController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ServiceRequestController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Create(int serviceId)
        {
            var service = await _context.Services
                .FirstOrDefaultAsync(s =>
                    s.ServiceId == serviceId &&
                    s.IsActive);

            if (service == null)
                return NotFound();

            ViewBag.Service = service;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            int serviceId,
            string requestDetails)
        {
            if (string.IsNullOrWhiteSpace(requestDetails))
            {
                ModelState.AddModelError(
                    "RequestDetails",
                    "Please describe what you need.");

                var service = await _context.Services
                    .FirstOrDefaultAsync(s =>
                        s.ServiceId == serviceId &&
                        s.IsActive);

                ViewBag.Service = service;

                return View();
            }

            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized();

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c =>
                    c.ApplicationUserId == userId &&
                    c.IsActive);

            if (customer == null)
                return NotFound();

            var selectedService = await _context.Services
                .FirstOrDefaultAsync(s =>
                    s.ServiceId == serviceId &&
                    s.IsActive);

            if (selectedService == null)
                return NotFound();

            var request = new ServiceRequest
            {
                ServiceId = selectedService.ServiceId,
                CustomerId = customer.CustomerId,
                RequestDetails = requestDetails,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.ServiceRequests.Add(request);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Service request submitted successfully.";

            return RedirectToAction(nameof(MyRequests));
        }

        [HttpGet]
        public async Task<IActionResult> MyRequests()
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized();

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c =>
                    c.ApplicationUserId == userId &&
                    c.IsActive);

            if (customer == null)
                return NotFound();

            var requests = await _context.ServiceRequests
                .Include(r => r.Service)
                .Where(r => r.CustomerId == customer.CustomerId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(requests);
        }
    }
}