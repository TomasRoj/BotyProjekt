using BotyProjekt.Models;

namespace BotyProjekt.Admin.Models
{
    public class AdminViewModel
    {
        public string ActiveSection { get; set; } = "orders";

        public List<Product> Products { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public List<Order> Orders { get; set; } = new();
        public List<Color> Colors { get; set; } = new();
        public List<Size> Sizes { get; set; } = new();

        public Product NewProduct { get; set; }
        public Category NewCategory { get; set; }
    }
}
