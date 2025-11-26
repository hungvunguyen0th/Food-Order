using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asm_GD1.Models
{
    public class ProductSize
    {
        [Key]
        public int SizeID { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [NotMapped]
        public string SizeName
        {
            get => Name;
            set => Name = value;
        }

        public decimal ExtraPrice { get; set; }
    }
}
