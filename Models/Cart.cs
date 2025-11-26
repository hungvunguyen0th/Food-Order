using Asm_GD1.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Cart
{
    [Key]
    public int CartID { get; set; }

    public string? UserID { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    [NotMapped]
    public decimal TotalPrice => CartItems?.Sum(ci => ci.TotalPrice) ?? 0;

    [NotMapped]
    public decimal UnitPrice => CartItems?.Sum(ci => ci.UnitPrice) ?? 0;
}
