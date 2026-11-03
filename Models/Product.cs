using System.ComponentModel.DataAnnotations;

namespace BusinessManagementSystem.Models
{
    public class Product
    {
        public int ProductId { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [StringLength(50)]
        public string Type { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Category { get; set; }

        [Range(0, 999999999)]
        public decimal Price { get; set; }

        [Range(0, 999999999)]
        public decimal CostPrice { get; set; }

        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}