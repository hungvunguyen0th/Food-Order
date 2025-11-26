namespace Asm_GD1.Models
{
    public class ProductViewModel
    {
        public IEnumerable<Product> Product { get; set; } = new List<Product>();
        public List<ProductSize> Sizes { get; set; } = new List<ProductSize>();
        public List<ProductTopping> Toppings { get; set; } = new List<ProductTopping>();
    }
}
