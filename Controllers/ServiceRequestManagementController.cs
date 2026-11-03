using BusinessManagementSystem.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BusinessManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,Manager,Staff")]
    public class ServiceRequestManagementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ServiceRequestManagementController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? status)
        {
            var requests = _context.ServiceRequests
                .Include(r => r.Service)
                .Include(r => r.Customer)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                requests = requests.Where(r =>
                    r.Status == status);
            }

            var list = await requests
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            ViewBag.Status = status;

            return View(list);
        }


        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(
            int id,
            string status)
        {
            var allowedStatuses = new[]
            {
        "Pending",
        "Approved",
        "In Progress",
        "Completed",
        "Rejected"
    };

            if (string.IsNullOrWhiteSpace(status) ||
                !allowedStatuses.Contains(status))
            {
                return BadRequest();
            }

            var request = await _context.ServiceRequests
                .FindAsync(id);

            if (request == null)
            {
                return NotFound();
            }

            if (request.Status == "Completed" ||
                request.Status == "Rejected")
            {
                TempData["ErrorMessage"] =
                    "A completed or rejected request cannot be changed.";

                return RedirectToAction(nameof(Index));
            }

            if (request.Status == "Pending" &&
                status == "In Progress")
            {
                TempData["ErrorMessage"] =
                    "Approve the request before moving it to In Progress.";

                return RedirectToAction(nameof(Index));
            }

            if (request.Status == "Pending" &&
                status == "Completed")
            {
                TempData["ErrorMessage"] =
                    "Approve the request before completing it.";

                return RedirectToAction(nameof(Index));
            }

            if (request.Status == "Approved" &&
                status == "Pending")
            {
                TempData["ErrorMessage"] =
                    "An approved request cannot return to Pending.";

                return RedirectToAction(nameof(Index));
            }

            if (request.Status == "In Progress" &&
                (status == "Pending" || status == "Approved"))
            {
                TempData["ErrorMessage"] =
                    "An in-progress request cannot return to an earlier status.";

                return RedirectToAction(nameof(Index));
            }

            request.Status = status;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Service request status updated successfully.";

            return RedirectToAction(nameof(Index));
        }

    }
}