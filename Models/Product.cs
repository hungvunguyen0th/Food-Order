using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace Asm_GD1.Models
{
    public class Product
    {
        [Key]
        public int ProductID { get; set; }

        [Required(ErrorMessage = "Tên món ăn là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tên món ăn không được quá 200 ký tự")]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Giá sản phẩm là bắt buộc")]
        [Range(0, 999999999, ErrorMessage = "Giá phải lớn hơn 0")]
        public decimal BasePrice { get; set; }

        public decimal DiscountPrice { get; set; }

        [Range(0, 100, ErrorMessage = "Phần trăm giảm giá phải từ 0-100")]
        public int DiscountPercent { get; set; }

        [StringLength(500)]
        public string? ImageUrl { get; set; }
        [Required]
        public int CategoryID { get; set; }
        [StringLength(250)]
        public string? Slug { get; set; }

        public int SoldCount { get; set; } = 0;
        [Range(0, 5)]
        public decimal Rating { get; set; } = 0;
        [StringLength(500)]
        public string? NoteText { get; set; }
        public int? SizeID { get; set; }
        public int? ToppingID { get; set; }
        public bool IsAvailable { get; set; } = true;
        public int? PreparationTime { get; set; }
        public int? Calories { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [ForeignKey("SizeID")]
        public virtual ProductSize? Size { get; set; }

        [ForeignKey("ToppingID")]
        public virtual ProductTopping? Topping { get; set; }
        [NotMapped]
        [Display(Name = "Hình ảnh")]
        public IFormFile? ImageFile { get; set; }
        [ForeignKey("CategoryID")]
        public virtual Category? Category { get; set; }
    }
}
