
using BusinessManagementSystem.Data;
using BusinessManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BusinessManagementSystem.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PaymentController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Admin,Manager,Staff")]
        public async Task<IActionResult> Index()
        {
            var payments = await _context.Payments
                .Include(p => p.Invoice)
                    .ThenInclude(i => i!.Customer)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            return View(payments);
        }

        [Authorize(Roles = "Admin,Manager,Staff")]
        [HttpGet]
        public async Task<IActionResult> Create(int invoiceId)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Customer)
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);

            if (invoice == null)
                return NotFound();

            if (invoice.Status == "Cancelled")
                return BadRequest("Cancelled invoice cannot receive payments.");

            var paidAmount = await _context.Payments
                .Where(p => p.InvoiceId == invoiceId &&
                            p.Status == "Completed")
                .SumAsync(p => (decimal?)p.Amount) ?? 0m;

            var balance = invoice.TotalAmount - paidAmount;

            if (balance <= 0)
                return BadRequest("This invoice has no remaining balance.");

            ViewBag.Invoice = invoice;
            ViewBag.PaidAmount = paidAmount;
            ViewBag.Balance = balance;

            return View(new Payment
            {
                InvoiceId = invoiceId,
                Amount = balance,
                PaymentMethod = "Cash"
            });
        }

        [Authorize(Roles = "Admin,Manager,Staff")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            int invoiceId,
            decimal amount,
            string paymentMethod,
            string? notes)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Customer)
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);

            if (invoice == null)
                return NotFound();

            var allowedMethods = new[]
            {
                "Cash", "Bank Transfer", "Card", "Online"
            };

            var paidAmount = await _context.Payments
                .Where(p => p.InvoiceId == invoiceId &&
                            p.Status == "Completed")
                .SumAsync(p => (decimal?)p.Amount) ?? 0m;

            var balance = invoice.TotalAmount - paidAmount;

            if (invoice.Status == "Cancelled")
            {
                TempData["ErrorMessage"] =
                    "Cancelled invoice par payment record nahi ho sakti.";

                return RedirectToAction(nameof(Index));
            }

            if (balance <= 0)
            {
                TempData["ErrorMessage"] =
                    "Invoice ka full payment already receive ho chuka hai.";

                return RedirectToAction(nameof(Index));
            }

            if (!allowedMethods.Contains(paymentMethod))
                ModelState.AddModelError("PaymentMethod", "Select a valid payment method.");

            if (amount <= 0 || amount > balance)
                ModelState.AddModelError(
                    "Amount",
                    $"Enter an amount between Rs. 0.01 and Rs. {balance:N2}.");

            if (string.IsNullOrWhiteSpace(paymentMethod))
                ModelState.AddModelError("PaymentMethod", "Payment method is required.");

            if (notes?.Length > 500)
                ModelState.AddModelError("Notes", "Notes cannot exceed 500 characters.");

            if (!ModelState.IsValid)
            {
                ViewBag.Invoice = invoice;
                ViewBag.PaidAmount = paidAmount;
                ViewBag.Balance = balance;

                return View("Create", new Payment
                {
                    InvoiceId = invoiceId,
                    Amount = amount,
                    PaymentMethod = paymentMethod ?? "",
                    Notes = notes
                });
            }

            var payment = new Payment
            {
                InvoiceId = invoiceId,
                Amount = amount,
                PaymentMethod = paymentMethod,
                Notes = notes,
                TransactionReference =
                    $"PAY-{Guid.NewGuid():N}"[..16].ToUpperInvariant(),
                Status = "Completed",
                PaymentDate = DateTime.UtcNow
            };

            _context.Payments.Add(payment);

            var newPaidAmount = paidAmount + amount;

            invoice.Status = newPaidAmount >= invoice.TotalAmount
                ? "Paid"
                : "Unpaid";

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Payment recorded successfully.";

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var payment = await _context.Payments
                .Include(p => p.Invoice)
                .FirstOrDefaultAsync(p => p.PaymentId == id);

            if (payment == null)
                return NotFound();

            if (payment.Status != "Completed")
            {
                TempData["ErrorMessage"] = "This payment is already cancelled.";
                return RedirectToAction(nameof(Index));
            }

            payment.Status = "Cancelled";

            var invoice = payment.Invoice;

            if (invoice != null && invoice.Status != "Cancelled")
            {
                var remainingPaidAmount = await _context.Payments
                    .Where(p => p.InvoiceId == payment.InvoiceId &&
                                p.PaymentId != payment.PaymentId &&
                                p.Status == "Completed")
                    .SumAsync(p => (decimal?)p.Amount) ?? 0m;

                invoice.Status =
                    remainingPaidAmount >= invoice.TotalAmount
                    ? "Paid"
                    : "Unpaid";
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Payment cancelled and invoice balance recalculated.";

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> MyPayments()
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

            var payments = await _context.Payments
                .Include(p => p.Invoice)
                .Where(p => p.Invoice != null &&
                            p.Invoice.CustomerId == customer.CustomerId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            return View(payments);
        }
    }
}
