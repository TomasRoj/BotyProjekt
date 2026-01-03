using BotyProjekt.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BotyProjekt.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class CategoriesController : Controller
    {
        private readonly MyContext _context;

        public CategoriesController(MyContext context)
        {
            _context = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category newCategory)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(newCategory.Name))
                {
                    return RedirectToAction("Index", "Dashboard", new { section = "categories" });
                }

                newCategory.Name = newCategory.Name.Trim();

                _context.Categories.Add(newCategory);
                await _context.SaveChangesAsync();

            }
            catch (Exception)
            {
                TempData["Error"] = "Neočekávaná chyba při vytváření kategorie.";
            }

            return RedirectToAction("Index", "Dashboard", new { section = "categories" });
        }

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var category = await _context.Categories
                    .Include(c => c.SubCategories)
                    .Include(c => c.Products)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category == null)
                {
                    return RedirectToAction("Index", "Dashboard", new { section = "categories" });
                }

                if (category.Products.Count > 0)
                {
                    return RedirectToAction("Index", "Dashboard", new { section = "categories" });
                }
                if (category.SubCategories.Count > 0)
                {
                    foreach (var subCategory in category.SubCategories)
                    {
                        subCategory.ParentId = null;
                    }
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

            }
            catch (Exception)
            {
                TempData["Error"] = "Neočekávaná chyba při mazání kategorie.";
            }

            return RedirectToAction("Index", "Dashboard", new { section = "categories" });
        }
    }
}

