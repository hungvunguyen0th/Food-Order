using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asm_GD1.Models
{
    public class CartItem
    {
        [Key]
        public int CartItemID { get; set; }

        public int CartID { get; set; }
        public int ProductID { get; set; }
        public int? SizeID { get; set; }
        public int? ToppingID { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string? ToppingIDs { get; set; }
        public string? Note { get; set; }
        public string? ProductName { get; set; }
        public string? ProductImage { get; set; }
        public string? SizeName { get; set; }
        public string? ToppingName { get; set; } 
        public decimal UnitPrice { get; set; }   
        public decimal TotalPrice                 
        {
            get => Price * Quantity;
        }

        [ForeignKey("CartID")]
        public virtual Cart Cart { get; set; } = null!;

        [ForeignKey("ProductID")]
        public virtual Product Product { get; set; } = null!;

        [ForeignKey("SizeID")]
        public virtual ProductSize? Size { get; set; }

        [ForeignKey("ToppingID")]
        public virtual ProductTopping? Topping { get; set; }
    }

}
