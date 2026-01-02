using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.Linq;
using System.Collections.Generic;
using System;
using BotyProjekt.Models;

namespace BotyProjekt.Controllers
{
    public class CartController : Controller
    {
        private const string SessionCartKey = "Cart";
        private readonly MyContext _context;

        public CartController(MyContext context)
        {
            _context = context;
        }

        private List<CartViewItem> GetCartFromSession()
        {
            var json = HttpContext.Session.GetString(SessionCartKey);
            if (string.IsNullOrEmpty(json))
            {
                return new List<CartViewItem>();
            }

            try
            {
                return JsonSerializer.Deserialize<List<CartViewItem>>(json) ?? new List<CartViewItem>();
            }
            catch
            {
                return new List<CartViewItem>();
            }
        }

        private void SaveCartToSession(List<CartViewItem> cart)
        {
            HttpContext.Session.SetString(SessionCartKey, JsonSerializer.Serialize(cart));
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(int productId, string name, decimal price, string imageUrl, int quantity = 1)
        {
            var cart = GetCartFromSession();
            var existing = cart.FirstOrDefault(x => x.ProductId == productId);
            if (existing != null)
            {
                existing.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartViewItem
                {
                    ProductId = productId,
                    Name = name ?? string.Empty,
                    Price = price,
                    ImageUrl = imageUrl ?? string.Empty,
                    Quantity = Math.Max(1, quantity)
                });
            }

            SaveCartToSession(cart);
            TempData["CartMessage"] = "Položka přidána do košíku.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateQuantity(int productId, int quantity)
        {
            if (quantity < 1)
            {
                quantity = 1;
            }

            var cart = GetCartFromSession();
            var item = cart.FirstOrDefault(x => x.ProductId == productId);
            if (item != null)
            {
                item.Quantity = quantity;
                SaveCartToSession(cart);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(int productId)
        {
            var cart = GetCartFromSession();
            var item = cart.FirstOrDefault(x => x.ProductId == productId);
            if (item != null)
            {
                cart.Remove(item);
                SaveCartToSession(cart);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CheckoutSubmit(string jmeno, string prijmeni, string email, string telefon, string address, string zeme, string platba)
        {
            var cart = GetCartFromSession();
            if (!cart.Any())
            {
                TempData["CartMessage"] = "Košík je prázdný.";
                return RedirectToAction("Index");
            }

            if (string.IsNullOrWhiteSpace(jmeno) || string.IsNullOrWhiteSpace(prijmeni) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(address))
            {
                TempData["CartMessage"] = "Vyplňte prosím všechny povinné údaje.";
                return RedirectToAction("Index");
            }

            try
            {
                string customerName = $"{jmeno} {prijmeni}";
                string customerAddress = $"{address}, {zeme}";
                decimal totalPrice = cart.Sum(x => x.Price * x.Quantity);

                var order = new Order
                {
                    OrderDate = DateTime.Now,
                    CustomerName = customerName,
                    CustomerEmail = email,
                    CustomerAddress = customerAddress,
                    TotalPrice = totalPrice
                };

                _context.Orders.Add(order);
                _context.SaveChanges();

                foreach (var item in cart)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        ProductVariantId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.Price
                    };

                    _context.OrderItems.Add(orderItem);
                }

                _context.SaveChanges();

                HttpContext.Session.Remove(SessionCartKey);
                TempData["CartMessage"] = "Děkujeme — poptávka byla odeslána.";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                TempData["CartMessage"] = $"Chyba při ukládání objednávky: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        public class CartViewItem
        {
            public int ProductId { get; set; }
            public string Name { get; set; } = string.Empty;
            public string ImageUrl { get; set; } = string.Empty;
            public decimal Price { get; set; }
            public int Quantity { get; set; }
        }
    }
}
