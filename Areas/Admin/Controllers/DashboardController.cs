using BotyProjekt.Admin.Models;
using BotyProjekt.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BotyProjekt.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly MyContext _context;

        public DashboardController(MyContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string section = "orders")
        {
            var model = new AdminViewModel
            {
                ActiveSection = section,
                Orders = await _context.Orders.OrderByDescending(o => o.OrderDate).ToListAsync(),
                Products = await _context.Products.Include(p => p.Category).ToListAsync(),
                Categories = await _context.Categories.ToListAsync(),
                Colors = await _context.Colors.ToListAsync(),
                Sizes = await _context.Sizes.OrderBy(s => s.Name).ToListAsync()
            };

            ViewBag.CategoriesList = new SelectList(model.Categories, "Id", "Name");

            return View(model);
        }
    }
}
