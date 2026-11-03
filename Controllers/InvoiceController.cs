using BusinessManagementSystem.Data;
using BusinessManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BusinessManagementSystem.Controllers
{
    [Authorize]
    public class InvoiceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InvoiceController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Admin,Manager,Staff")]
        public async Task<IActionResult> Index(string? status)
        {
            var invoices = _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Order)
                .Include(i => i.InvoiceItems)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                invoices = invoices.Where(i => i.Status == status);
            }

            ViewBag.Status = status;

            return View(await invoices
                .OrderByDescending(i => i.InvoiceDate)
                .ToListAsync());
        }

        [Authorize(Roles = "Admin,Manager,Staff")]
        [HttpGet]
        public async Task<IActionResult> Create(int? orderId)
        {
            if (!orderId.HasValue)
            {
                return BadRequest();
            }

            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Product)
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Service)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                return NotFound();
            }
            if (order.Status == "Cancelled")
            {
                TempData["ErrorMessage"] =
                    "Cancelled order ke liye invoice generate nahi kar sakte.";

                return RedirectToAction(nameof(Index));
            }

            var existingInvoice = await _context.Invoices
                .AnyAsync(i => i.OrderId == orderId);

            if (existingInvoice)
            {
                TempData["ErrorMessage"] =
                    "An invoice already exists for this order.";

                return RedirectToAction(nameof(Index));
            }

            return View(order);
        }

        [Authorize(Roles = "Admin,Manager,Staff")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Product)
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Service)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                return NotFound();
            }

            if (order.Status == "Cancelled")
            {
                TempData["ErrorMessage"] =
                    "This invoice has recorded payments. Reverse or refund the payments before cancelling the invoice.";

                return RedirectToAction(nameof(Index));
            }

            var existingInvoice = await _context.Invoices
                .AnyAsync(i => i.OrderId == orderId);

            if (existingInvoice)
            {
                TempData["ErrorMessage"] =
                    "An invoice already exists for this order.";

                return RedirectToAction(nameof(Index));
            }

            var subTotal = order.OrderItems.Sum(i => i.TotalPrice);

            var invoice = new Invoice
            {
                InvoiceNumber =
                    $"INV-{DateTime.UtcNow:yyyyMMddHHmmssfff}",

                OrderId = order.OrderId,
                CustomerId = order.CustomerId,
                InvoiceDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(7),
                Status = "Unpaid",
                SubTotal = subTotal,
                TaxAmount = 0,
                TotalAmount = subTotal
            };

            foreach (var item in order.OrderItems)
            {
                var description = item.Service != null
                    ? item.Service.Name
                    : item.Product != null
                        ? item.Product.Name
                        : "Order Item";

                invoice.InvoiceItems.Add(new InvoiceItem
                {
                    Description = description,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.TotalPrice
                });
            }

            _context.Invoices.Add(invoice);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Invoice generated successfully.";

            return RedirectToAction(nameof(Details),
                new { id = invoice.InvoiceId });
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
                "Unpaid",
                "Cancelled"
            };

            if (!allowedStatuses.Contains(status))
            {
                return BadRequest();
            }

            var invoice = await _context.Invoices
                .FindAsync(id);

            if (invoice == null)
            {
                return NotFound();
            }

            if (status == "Cancelled")
            {
                var hasCompletedPayments = await _context.Payments
                    .AnyAsync(p =>
                        p.InvoiceId == id &&
                        p.Status == "Completed");

                if (hasCompletedPayments)
                {
                    TempData["ErrorMessage"] =
                        "Payment recorded hai. Invoice cancel karne se pehle payment reversal handle karein.";

                    return RedirectToAction(nameof(Index));
                }
            }

            if (invoice.Status == "Cancelled" && status != "Cancelled")
            {
                TempData["ErrorMessage"] =
                    "A cancelled invoice cannot be reopened.";
                return RedirectToAction(nameof(Index));
            }

            if (invoice.Status == "Paid" && status == "Unpaid")
            {
                TempData["ErrorMessage"] =
                    "Paid invoice ko manually Unpaid nahi kar sakte.";

                return RedirectToAction(nameof(Index));
            }

            invoice.Status = status;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Invoice status updated.";

            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invoice = await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Order)
                .Include(i => i.InvoiceItems)
                .FirstOrDefaultAsync(i => i.InvoiceId == id);

            if (invoice == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Customer"))
            {
                var userId = User.FindFirst(
                    System.Security.Claims.ClaimTypes.NameIdentifier)
                    ?.Value;

                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c =>
                        c.ApplicationUserId == userId &&
                        c.IsActive);

                if (customer == null ||
                    invoice.CustomerId != customer.CustomerId)
                {
                    return Forbid();
                }
            }

            return View(invoice);
        }

        [Authorize(Roles = "Customer")]
        [HttpGet]
        public async Task<IActionResult> MyInvoices()
        {
            var userId = User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier)
                ?.Value;

            if (userId == null)
            {
                return Unauthorized();
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c =>
                    c.ApplicationUserId == userId &&
                    c.IsActive);

            if (customer == null)
            {
                return NotFound();
            }

            var invoices = await _context.Invoices
                .Where(i => i.CustomerId == customer.CustomerId)
                .OrderByDescending(i => i.InvoiceDate)
                .ToListAsync();

            return View(invoices);
        }
    }
}