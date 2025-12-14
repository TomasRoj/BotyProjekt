using BotyProjekt.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BotyProjekt.ViewModels;

namespace BotyProjekt.Controllers
{
    public class ProductController : Controller
    {
        private readonly MyContext context = new MyContext();

        public IActionResult Index(
            int? categoryId,
            string sortOrder,
            decimal? minPrice,
            decimal? maxPrice,
            List<int> selectedColorIds,
            List<int> selectedSizeIds,
            int page = 1)
        {
            int pageSize = 9;

            var query = context.Products
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .ThenInclude(v => v.Color)
                .Include(p => p.Variants)
                .ThenInclude(v => v.Size)
                .AsQueryable();


            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Variants.Any(v => v.Price >= minPrice.Value));
            }
            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Variants.Any(v => v.Price <= maxPrice.Value));
            }

            if (selectedColorIds != null && selectedColorIds.Any())
            {
                query = query.Where(p => p.Variants.Any(v => selectedColorIds.Contains(v.ColorId)));
            }

            if (selectedSizeIds != null && selectedSizeIds.Any())
            {
                query = query.Where(p => p.Variants.Any(v => selectedSizeIds.Contains(v.SizeId)));
            }

            switch (sortOrder)
            {
                case "price_asc":
                    query = query.OrderBy(p => p.Variants.Min(v => v.Price));
                    break;
                case "price_desc":
                    query = query.OrderByDescending(p => p.Variants.Min(v => v.Price));
                    break;
                case "name":
                    query = query.OrderBy(p => p.Name);
                    break;
                default:
                    query = query.OrderByDescending(p => p.Id);
                    break;
            }

            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var products = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var viewModel = new ListViewModel
            {
                Products = products,
                Categories = context.Categories.Include(c => c.SubCategories).ToList(),
                Colors = context.Colors.ToList(),
                Sizes = context.Sizes.OrderBy(s => s.Name).ToList(),

                CurrentCategoryId = categoryId,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                SelectedColorIds = selectedColorIds ?? new List<int>(),
                SelectedSizeIds = selectedSizeIds ?? new List<int>(),
                SortOrder = sortOrder,
                CurrentPage = page,
                TotalPages = totalPages
            };

            return View(viewModel);
        }

        public IActionResult Detail(int id)
            {
                var product = context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Images)
                    .Include(p => p.Variants)
                    .ThenInclude(v => v.Color)
                    .Include(p => p.Variants)
                    .ThenInclude(v => v.Size)
                    .FirstOrDefault(p => p.Id == id);

                if (product == null)
                {
                    return NotFound();
                }

                var relatedProducts = context.Products
                    .Include(p => p.Images)
                    .Include(p => p.Variants)
                    .Where(p => p.CategoryId == product.CategoryId && p.Id != id)
                    .Take(4)
                    .ToList();

                ViewBag.RelatedProducts = relatedProducts;

                return View(product);
            }
        }
}
