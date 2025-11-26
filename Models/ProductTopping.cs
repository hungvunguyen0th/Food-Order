using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asm_GD1.Models
{
    public class ProductTopping
    {
        [Key]
        public int ToppingID { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [NotMapped]
        public string ToppingName
        {
            get => Name;
            set => Name = value;
        }

        public decimal ExtraPrice { get; set; }
    }
}
