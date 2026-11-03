
using System.ComponentModel.DataAnnotations;

namespace BusinessManagementSystem.ViewModels
{
    public class CreateOrderViewModel
    {
        [Required]
        public int CustomerId { get; set; }

        [Required]
        public string ItemType { get; set; } = "Product";

        [Range(1, int.MaxValue)]
        public int ItemId { get; set; }

        [Range(1, 100000)]
        public int Quantity { get; set; } = 1;
    }
}
