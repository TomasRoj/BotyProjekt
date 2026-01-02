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
                    TempData["Error"] = "Název kategorie je povinný.";
                    return RedirectToAction("Index", "Dashboard", new { section = "categories" });
                }

                newCategory.Name = newCategory.Name.Trim();

                _context.Categories.Add(newCategory);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Kategorie byla úspěšně vytvořena.";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Chyba při ukládání kategorie do databáze.";
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
                    TempData["Warning"] = "Kategorie nebyla nalezena.";
                    return RedirectToAction("Index", "Dashboard", new { section = "categories" });
                }

                // Kontrola, zda kategorie obsahuje produkty
                if (category.Products.Count > 0)
                {
                    TempData["Error"] = $"Nelze smazat kategorii '{category.Name}'. Obsahuje {category.Products.Count} produkt(ů). Nejprve přesuň nebo smaž produkty.";
                    return RedirectToAction("Index", "Dashboard", new { section = "categories" });
                }

                // Smaž podkategorie (pokud mají samy sebe jako rodiče)
                if (category.SubCategories.Count > 0)
                {
                    // Nastav parentId na null pro všechny podkategorie
                    foreach (var subCategory in category.SubCategories)
                    {
                        subCategory.ParentId = null;
                    }
                }

                // Smaž samotnou kategorii
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Kategorie byla úspěšně smazána.";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Chyba při mazání kategorie z databáze.";
            }
            catch (Exception)
            {
                TempData["Error"] = "Neočekávaná chyba při mazání kategorie.";
            }

            return RedirectToAction("Index", "Dashboard", new { section = "categories" });
        }
    }
}

