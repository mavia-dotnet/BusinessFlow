using System.ComponentModel.DataAnnotations;

namespace BusinessManagementSystem.Models
{
    public class ServiceRequest
    {
        public int ServiceRequestId { get; set; }

        [Required]
        public int ServiceId { get; set; }

        public Service? Service { get; set; }

        [Required]
        public int CustomerId { get; set; }

        public Customer? Customer { get; set; }

        [Required]
        [StringLength(1000)]
        public string RequestDetails { get; set; } = string.Empty;

        [StringLength(30)]
        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}