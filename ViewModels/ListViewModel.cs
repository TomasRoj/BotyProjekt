using BotyProjekt.Models;

namespace BotyProjekt.ViewModels
{
    public class ListViewModel
    {
        public List<Product> Products { get; set; }

        public List<Category> Categories { get; set; }
        public List<Color> Colors { get; set; }
        public List<Size> Sizes { get; set; }

        public int? CurrentCategoryId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public List<int> SelectedColorIds { get; set; } = new List<int>();
        public List<int> SelectedSizeIds { get; set; } = new List<int>();

        public string SortOrder { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
