using System.ComponentModel.DataAnnotations;

namespace BusinessManagementSystem.Models
{
    public class InvoiceItem
    {
        public int InvoiceItemId { get; set; }

        [Required]
        public int InvoiceId { get; set; }

        public Invoice? Invoice { get; set; }

        [StringLength(200)]
        public string Description { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; } = 1;

        [Range(0, 999999999)]
        public decimal UnitPrice { get; set; }

        [Range(0, 999999999)]
        public decimal TotalPrice { get; set; }
    }
}