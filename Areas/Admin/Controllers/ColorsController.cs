using BotyProjekt.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BotyProjekt.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class ColorsController : Controller
    {
        private readonly MyContext _context;

        public ColorsController(MyContext context)
        {
            _context = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string name, string hexCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(hexCode))
                {
                    return RedirectToAction("Index", "Dashboard", new { section = "colors" });
                }

                var color = new Color
                {
                    Name = name.Trim(),
                    HexCode = hexCode.Trim()
                };

                _context.Colors.Add(color);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                TempData["Error"] = "Chyba pøi ukládání barvy.";
            }

            return RedirectToAction("Index", "Dashboard", new { section = "colors" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var color = await _context.Colors
                    .Include(c => c.Variants)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (color == null)
                {
                    return RedirectToAction("Index", "Dashboard", new { section = "colors" });
                }

                if (color.Variants.Count > 0)
                {
                    return RedirectToAction("Index", "Dashboard", new { section = "colors" });
                }

                _context.Colors.Remove(color);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                TempData["Error"] = "Chyba pøi mazání barvy.";
            }

            return RedirectToAction("Index", "Dashboard", new { section = "colors" });
        }
    }
}