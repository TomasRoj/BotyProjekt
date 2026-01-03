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
                    return RedirectToAction("Index", "Dashboard", new { section = "sizes" });
                }

                var size = new Size
                {
                    Name = name.Trim()
                };

                _context.Sizes.Add(size);
                await _context.SaveChangesAsync();
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
                    return RedirectToAction("Index", "Dashboard", new { section = "sizes" });
                }

                if (size.Variants.Count > 0)
                {
                    return RedirectToAction("Index", "Dashboard", new { section = "sizes" });
                }

                _context.Sizes.Remove(size);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                TempData["Error"] = "Chyba pøi mazání velikosti.";
            }

            return RedirectToAction("Index", "Dashboard", new { section = "sizes" });
        }
    }
}