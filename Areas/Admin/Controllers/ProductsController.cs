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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string name, string description, decimal basePrice, int discount, int categoryId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    TempData["Error"] = "Název produktu je povinný.";
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin", section = "products" });
                }

                var product = new Product
                {
                    Name = name.Trim(),
                    Description = description?.Trim() ?? string.Empty,
                    BasePrice = basePrice,
                    Discount = discount,
                    CategoryId = categoryId
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Produkt byl úspěšně vytvořen.";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Chyba při ukládání produktu do databáze.";
            }
            catch (Exception)
            {
                TempData["Error"] = "Neočekávaná chyba při vytváření produktu.";
            }

            return RedirectToAction("Index", "Dashboard", new { area = "Admin", section = "products" });
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.Category)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null)
                {
                    TempData["Warning"] = "Produkt nebyl nalezen.";
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin", section = "products" });
                }

                ViewBag.Categories = await _context.Categories.ToListAsync();
                return View(product);
            }
            catch (Exception)
            {
                TempData["Error"] = "Chyba při načítání produktu.";
                return RedirectToAction("Index", "Dashboard", new { area = "Admin", section = "products" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSubmit(int id, string name, string description, decimal basePrice, int discount, int categoryId)
        {
            try
            {
                var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);

                if (product == null)
                {
                    TempData["Warning"] = "Produkt nebyl nalezen.";
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin", section = "products" });
                }

                if (string.IsNullOrWhiteSpace(name))
                {
                    TempData["Error"] = "Název produktu je povinný.";
                    return RedirectToAction("Edit", new { area = "Admin", id });
                }

                product.Name = name.Trim();
                product.Description = description?.Trim() ?? string.Empty;
                product.BasePrice = basePrice;
                product.Discount = discount;
                product.CategoryId = categoryId;

                _context.Products.Update(product);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Produkt byl úspěšně aktualizován.";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Chyba při ukládání produktu do databáze.";
            }
            catch (Exception)
            {
                TempData["Error"] = "Neočekávaná chyba při editaci produktu.";
            }

            return RedirectToAction("Index", "Dashboard", new { area = "Admin", section = "products" });
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
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin", section = "products" });
                }

                var variantIds = product.Variants.Select(v => v.Id).ToList();
                if (variantIds.Count > 0)
                {
                    var orderItems = await _context.OrderItems
                        .Where(oi => variantIds.Contains(oi.ProductVariantId))
                        .ToListAsync();

                    _context.OrderItems.RemoveRange(orderItems);
                }

                _context.ProductVariants.RemoveRange(product.Variants);
                _context.ProductImages.RemoveRange(product.Images);
                _context.Products.Remove(product);

                await _context.SaveChangesAsync();

                TempData["Success"] = "Produkt a všechna související data byla úspěšně smazána.";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Chyba při mazání produktu z databáze.";
            }
            catch (Exception)
            {
                TempData["Error"] = "Neočekávaná chyba při mazání produktu.";
            }

            return RedirectToAction("Index", "Dashboard", new { area = "Admin", section = "products" });
        }
    }
}
