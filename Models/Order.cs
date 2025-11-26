using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asm_GD1.Models
{
    public class Order
    {
        [Key]
        public int OrderID { get; set; }

        public string? UserID { get; set; }

        [Required]
        [StringLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string CustomerPhone { get; set; } = string.Empty;

        [StringLength(100)]
        public string? CustomerEmail { get; set; }

        [StringLength(500)]
        public string? ShippingAddress { get; set; }

        [StringLength(500)]
        public string? Note { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending";

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; } = "Cash";

        [Required]
        [StringLength(50)]
        public string PaymentStatus { get; set; } = "Pending";

        public decimal SubTotal { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }

        [StringLength(50)]
        public string? DiscountCode { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [ForeignKey("UserID")]
        public virtual ApplicationUser? User { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
