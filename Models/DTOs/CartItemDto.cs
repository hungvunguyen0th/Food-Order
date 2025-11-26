namespace Asm_GD1.Models.DTOs
{
    public class CartDto
    {
        public int CartID { get; set; }
        public string? UserID { get; set; }
        public List<CartItemDto> Items { get; set; } = new();
        public decimal TotalPrice { get; set; }
        public int TotalItems { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CartItemDto
    {
        public int CartItemID { get; set; }
        public int ProductID { get; set; }
        public string? ProductName { get; set; }
        public string? ProductImage { get; set; }
        public int? SizeID { get; set; }
        public string? SizeName { get; set; }
        public int? ToppingID { get; set; }
        public string? ToppingName { get; set; }
        public string? ToppingIDs { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string? Note { get; set; }
    }

    public class AddToCartDto
    {
        public int ProductID { get; set; }
        public int? SizeID { get; set; }
        public int[]? ToppingIds { get; set; }
        public int Quantity { get; set; } = 1;
        public string? Note { get; set; }
    }

    public class UpdateCartItemDto
    {
        public int CartItemID { get; set; }
        public int Quantity { get; set; }
    }
}
