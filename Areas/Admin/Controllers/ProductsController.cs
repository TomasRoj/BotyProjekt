using BotyProjekt.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BotyProjekt.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly MyContext _context;

        public ProductsController(MyContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product newProduct)
        {
            try
            {
                // Odstraň validaci pro Category (předpokládáme, že to přijde ze formuláře)
                ModelState.Remove("Category");

                if (ModelState.IsValid)
                {
                    _context.Products.Add(newProduct);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Produkt byl úspěšně vytvořen.";
                }
                else
                {
                    TempData["Error"] = "Chyba při validaci produktu. Zkontroluj vsechá povinná pole.";
                }
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Chyba při ukládání produktu do databáze.";
            }
            catch (Exception)
            {
                TempData["Error"] = "Neočekávaná chyba při vytváření produktu.";
            }

            return RedirectToAction("Index", "Dashboard", new { section = "products" });
        }

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.Variants)
                    .Include(p => p.Images)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null)
                {
                    TempData["Warning"] = "Produkt nebyl nalezen.";
                    return RedirectToAction("Index", "Dashboard", new { section = "products" });
                }

                // Smaž OrderItems spojené s ProductVariants tohoto produktu
                var variantIds = product.Variants.Select(v => v.Id).ToList();
                if (variantIds.Count > 0)
                {
                    var orderItems = await _context.OrderItems
                        .Where(oi => variantIds.Contains(oi.ProductVariantId))
                        .ToListAsync();

                    _context.OrderItems.RemoveRange(orderItems);
                }

                // Smaž ProductVariants
                _context.ProductVariants.RemoveRange(product.Variants);

                // Smaž ProductImages
                _context.ProductImages.RemoveRange(product.Images);

                // Smaž samotný Product
                _context.Products.Remove(product);

                await _context.SaveChangesAsync();

                TempData["Success"] = "Produkt a všechny související data byla úspěšně smazána.";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Chyba při mazání produktu z databáze.";
            }
            catch (Exception)
            {
                TempData["Error"] = "Neočekávaná chyba při mazání produktu.";
            }

            return RedirectToAction("Index", "Dashboard", new { section = "products" });
        }
    }
}
