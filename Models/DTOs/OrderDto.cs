namespace Asm_GD1.Models.DTOs
{
    public class OrderDto
    {
        public int OrderID { get; set; }
        public string? UserID { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerEmail { get; set; }
        public string? ShippingAddress { get; set; }
        public string? Note { get; set; }
        public string Status { get; set; } = "Pending";
        public string PaymentMethod { get; set; } = "Cash";
        public string PaymentStatus { get; set; } = "Pending";
        public decimal SubTotal { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class OrderItemDto
    {
        public int OrderItemID { get; set; }
        public int ProductID { get; set; }
        public string? ProductName { get; set; }
        public string? ProductImage { get; set; }
        public string? SizeName { get; set; }
        public string? ToppingName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string? Note { get; set; }
    }

    public class CreateOrderDto
    {
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string? CustomerEmail { get; set; }
        public string? ShippingAddress { get; set; }
        public string? Note { get; set; }
        public string PaymentMethod { get; set; } = "Cash";
        public decimal ShippingFee { get; set; }
        public string? DiscountCode { get; set; }
    }

    public class UpdateOrderStatusDto
    {
        public string Status { get; set; } = string.Empty;
    }
}
