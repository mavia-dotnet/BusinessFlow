using System.ComponentModel.DataAnnotations;

namespace BusinessManagementSystem.Models
{
    public class Order
    {
        public int OrderId { get; set; }

        [Required]
        public int CustomerId { get; set; }

        public Customer? Customer { get; set; }

        [StringLength(30)]
        public string Status { get; set; } = "Pending";

        [Range(0, 999999999)]
        public decimal TotalAmount { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public ICollection<OrderItem> OrderItems { get; set; }
            = new List<OrderItem>();
    }
}