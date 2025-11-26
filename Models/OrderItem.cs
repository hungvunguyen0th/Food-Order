using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asm_GD1.Models
{
    public class OrderItem
    {
        [Key]
        public int OrderItemID { get; set; }

        public int OrderID { get; set; }
        public int ProductID { get; set; }

        [StringLength(200)]
        public string? ProductName { get; set; }

        [StringLength(500)]
        public string? ProductImage { get; set; }

        [StringLength(50)]
        public string? SizeName { get; set; }

        [StringLength(200)]
        public string? ToppingName { get; set; }

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }

        [StringLength(500)]
        public string? Note { get; set; }

        [ForeignKey("OrderID")]
        public virtual Order Order { get; set; } = null!;

        [ForeignKey("ProductID")]
        public virtual Product Product { get; set; } = null!;
    }
}
