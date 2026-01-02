using BotyProjekt.Models;

namespace BotyProjekt.Admin.Models
{
    public class AdminViewModel
    {
        public string ActiveSection { get; set; } = "orders";

        public List<Product> Products { get; set; }
        public List<Category> Categories { get; set; }
        public List<Order> Orders { get; set; }

        public Product NewProduct { get; set; }
        public Category NewCategory { get; set; }
    }
}
