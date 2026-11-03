using BusinessManagementSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace BusinessManagementSystem.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }

        [Required]
        public int InvoiceId { get; set; }

        public Invoice? Invoice { get; set; }

        [Required]
        [StringLength(50)]
        public string TransactionReference { get; set; } = string.Empty;

        [Range(typeof(decimal), "0.01", "999999999")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(30)]
        public string PaymentMethod { get; set; } = "Cash";

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Completed";

        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
