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
                    .Include(p => p.Variants)
                    .ThenInclude(v => v.Color)
                    .Include(p => p.Variants)
                    .ThenInclude(v => v.Size)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null)
                {
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin", section = "products" });
                }

                ViewBag.Categories = await _context.Categories.ToListAsync();
                ViewBag.Colors = await _context.Colors.ToListAsync();
                ViewBag.Sizes = await _context.Sizes.ToListAsync();
                ViewBag.Variants = product.Variants.OrderBy(v => v.Color.Name).ThenBy(v => v.Size.Name).ToList();

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
        public async Task<IActionResult> EditSubmit(int id, string name, string description, decimal basePrice, 
            int discount, int categoryId, string saveBasic, string addVariant, int newColorId, int newSizeId, decimal newPrice, int newStockQuantity)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.Variants)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null)
                {
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin", section = "products" });
                }

                if (string.IsNullOrWhiteSpace(name))
                {
                    return RedirectToAction("Edit", new { area = "Admin", id });
                }

                if (!string.IsNullOrEmpty(saveBasic) && saveBasic == "true")
                {
                    product.Name = name.Trim();
                    product.Description = description?.Trim() ?? string.Empty;
                    product.BasePrice = basePrice;
                    product.Discount = discount;
                    product.CategoryId = categoryId;

                    _context.Products.Update(product);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Základní informace byly úspěšně uloženy.";
                    return RedirectToAction("Edit", new { area = "Admin", id });
                }

                if (!string.IsNullOrEmpty(addVariant) && addVariant == "true")
                {
                    if (newColorId <= 0 || newSizeId <= 0)
                    {
                        TempData["Error"] = "Musíte vybrat barvu a velikost.";
                        return RedirectToAction("Edit", new { area = "Admin", id });
                    }

                    var existingVariant = product.Variants.FirstOrDefault(v => v.ColorId == newColorId && v.SizeId == newSizeId);
                    if (existingVariant != null)
                    {
                        TempData["Error"] = "Tato kombinace barvy a velikosti již existuje.";
                        return RedirectToAction("Edit", new { area = "Admin", id });
                    }

                    var newVariant = new ProductVariant
                    {
                        ProductId = product.Id,
                        ColorId = newColorId,
                        SizeId = newSizeId,
                        Price = newPrice,
                        StockQuantity = newStockQuantity
                    };

                    _context.ProductVariants.Add(newVariant);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Varianta byla úspěšně přidána.";
                    return RedirectToAction("Edit", new { area = "Admin", id });
                }

                return RedirectToAction("Edit", new { area = "Admin", id });
            }
            catch (Exception)
            {
                TempData["Error"] = "Neočekávaná chyba při editaci produktu.";
            }

            return RedirectToAction("Edit", new { area = "Admin", id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteVariant(int id, int variantId)
        {
            try
            {
                var variant = await _context.ProductVariants
                    .FirstOrDefaultAsync(v => v.Id == variantId);

                if (variant == null)
                {
                    return RedirectToAction("Edit", new { area = "Admin", id });
                }

                var orderItems = await _context.OrderItems
                    .Where(oi => oi.ProductVariantId == variantId)
                    .ToListAsync();

                if (orderItems.Count > 0)
                {
                    TempData["Error"] = "Variantu nelze smazat, je použita v objednávkách.";
                    return RedirectToAction("Edit", new { area = "Admin", id });
                }

                _context.ProductVariants.Remove(variant);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Varianta byla úspěšně smazána.";
            }
            catch (Exception)
            {
                TempData["Error"] = "Neočekávaná chyba při mazání varianty.";
            }

            return RedirectToAction("Edit", new { area = "Admin", id });
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
            }
            catch (Exception)
            {
                TempData["Error"] = "Neočekávaná chyba při mazání produktu.";
            }

            return RedirectToAction("Index", "Dashboard", new { area = "Admin", section = "products" });
        }
    }
}
