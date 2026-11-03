using System.ComponentModel.DataAnnotations;

namespace BusinessManagementSystem.Models
{
    public class OrderItem
    {
        public int OrderItemId { get; set; }

        [Required]
        public int OrderId { get; set; }

        public Order? Order { get; set; }

        public int? ProductId { get; set; }

        public Product? Product { get; set; }

        public int? ServiceId { get; set; }

        public Service? Service { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; } = 1;

        [Range(0, 999999999)]
        public decimal UnitPrice { get; set; }

        [Range(0, 999999999)]
        public decimal TotalPrice { get; set; }

        public bool HasValidItemType()
        {
            return (ProductId.HasValue && !ServiceId.HasValue)
                || (!ProductId.HasValue && ServiceId.HasValue);
        }
    }
}