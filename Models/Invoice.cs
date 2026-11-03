using System.ComponentModel.DataAnnotations;

namespace BusinessManagementSystem.Models
{
    public class Invoice
    {
        public int InvoiceId { get; set; }

        [Required]
        [StringLength(30)]
        public string InvoiceNumber { get; set; } = string.Empty;

        [Required]
        public int OrderId { get; set; }

        public Order? Order { get; set; }

        [Required]
        public int CustomerId { get; set; }

        public Customer? Customer { get; set; }

        public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;

        public DateTime? DueDate { get; set; }

        [StringLength(30)]
        public string Status { get; set; } = "Unpaid";

        [Range(0, 999999999)]
        public decimal SubTotal { get; set; }

        [Range(0, 999999999)]
        public decimal TaxAmount { get; set; }

        [Range(0, 999999999)]
        public decimal TotalAmount { get; set; }

        public ICollection<InvoiceItem> InvoiceItems { get; set; }
            = new List<InvoiceItem>();
    }
}