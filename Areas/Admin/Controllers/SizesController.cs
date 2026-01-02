using BotyProjekt.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BotyProjekt.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class SizesController : Controller
    {
        private readonly MyContext _context;

        public SizesController(MyContext context)
        {
            _context = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    TempData["Error"] = "Název velikosti je povinný.";
                    return RedirectToAction("Index", "Dashboard", new { section = "sizes" });
                }

                var size = new Size
                {
                    Name = name.Trim()
                };

                _context.Sizes.Add(size);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Velikost byla úspìšnì pøidána.";
            }
            catch (Exception)
            {
                TempData["Error"] = "Chyba pøi ukládání velikosti.";
            }

            return RedirectToAction("Index", "Dashboard", new { section = "sizes" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var size = await _context.Sizes
                    .Include(s => s.Variants)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (size == null)
                {
                    TempData["Warning"] = "Velikost nebyla nalezena.";
                    return RedirectToAction("Index", "Dashboard", new { section = "sizes" });
                }

                if (size.Variants.Count > 0)
                {
                    TempData["Error"] = $"Nelze smazat velikost. Je používána v {size.Variants.Count} variantu(ch).";
                    return RedirectToAction("Index", "Dashboard", new { section = "sizes" });
                }

                _context.Sizes.Remove(size);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Velikost byla úspìšnì smazána.";
            }
            catch (Exception)
            {
                TempData["Error"] = "Chyba pøi mazání velikosti.";
            }

            return RedirectToAction("Index", "Dashboard", new { section = "sizes" });
        }
    }
}