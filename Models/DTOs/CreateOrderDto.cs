namespace Asm_GD1.Models.DTOs
{
    public class CreateOrderDtos
    {
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string? CustomerEmail { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public string? Note { get; set; }
        public string PaymentMethod { get; set; } = "Cash";
        public decimal ShippingFee { get; set; }
        public string? DiscountCode { get; set; }
    }
}