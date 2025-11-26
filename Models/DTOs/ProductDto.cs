namespace Asm_GD1.Models.DTOs
{
    public class ProductDto
    {
        public int ProductID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal BasePrice { get; set; }
        public decimal DiscountPrice { get; set; }
        public int DiscountPercent { get; set; }
        public string? ImageUrl { get; set; }
        public int CategoryID { get; set; }
        public string? CategoryName { get; set; }
        public string? Slug { get; set; }
        public int SoldCount { get; set; }
        public decimal Rating { get; set; }
        public bool IsAvailable { get; set; }
        public int? PreparationTime { get; set; }
        public int? Calories { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateProductDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal BasePrice { get; set; }
        public decimal DiscountPrice { get; set; }
        public int DiscountPercent { get; set; }
        public string? ImageUrl { get; set; }
        public int CategoryID { get; set; }
        public string? Slug { get; set; }
        public string? NoteText { get; set; }
        public bool IsAvailable { get; set; } = true;
        public int? PreparationTime { get; set; }
        public int? Calories { get; set; }
    }

    public class UpdateProductDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal BasePrice { get; set; }
        public decimal DiscountPrice { get; set; }
        public int DiscountPercent { get; set; }
        public string? ImageUrl { get; set; }
        public int CategoryID { get; set; }
        public string? Slug { get; set; }
        public string? NoteText { get; set; }
        public bool IsAvailable { get; set; }
        public int? PreparationTime { get; set; }
        public int? Calories { get; set; }
    }
}
