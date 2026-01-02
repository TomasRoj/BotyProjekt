using BotyProjekt.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BotyProjekt.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly MyContext _context;

        public OrdersController(MyContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Detail(int id)
        {
            try
            {
                var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id);

                if (order == null)
                {
                    TempData["Warning"] = "Objednávka nebyla nalezena.";
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin", section = "orders" });
                }

                var orderItems = await _context.OrderItems
                    .Where(oi => oi.OrderId == id)
                    .Include(oi => oi.ProductVariant)
                    .ThenInclude(pv => pv.Product)
                    .Include(oi => oi.ProductVariant)
                    .ThenInclude(pv => pv.Color)
                    .Include(oi => oi.ProductVariant)
                    .ThenInclude(pv => pv.Size)
                    .ToListAsync();

                ViewBag.Order = order;
                ViewBag.OrderItems = orderItems;

                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Chyba pøi naèítání detailù objednávky: {ex.Message}";
                return RedirectToAction("Index", "Dashboard", new { area = "Admin", section = "orders" });
            }
        }
    }
}