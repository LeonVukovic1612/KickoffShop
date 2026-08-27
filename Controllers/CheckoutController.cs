using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using KickoffShop.Data;
using KickoffShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KickoffShop.Controllers;

public class CheckoutController(AppDbContext db) : Controller
{
    private const string CartKey = "Cart";

    private List<CartItem> GetCart()
    {
        var json = HttpContext.Session.GetString(CartKey);
        return json is null ? [] : JsonSerializer.Deserialize<List<CartItem>>(json)!;
    }

    public async Task<IActionResult> Index()
    {
        var cart = GetCart();
        if (cart.Count == 0) return RedirectToAction("Index", "Cart");

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim != null)
        {
            var user = await db.Users.FindAsync(int.Parse(userIdClaim));
            if (user != null)
            {
                ViewBag.ProfileName = user.Name;
                ViewBag.ProfileEmail = user.Email;
                ViewBag.ProfileAddress = user.Address;
                ViewBag.ProfileCity = user.City;
            }
        }

        return View(cart);
    }

    [HttpPost]
    public async Task<IActionResult> PlaceOrder(string customerName, string customerEmail,
        string address, string city, string cardNumber, string cardExpiry, string cardCvv)
    {
        var cart = GetCart();
        if (cart.Count == 0) return RedirectToAction("Index", "Cart");

        var digits = (cardNumber ?? "").Replace(" ", "");
        if (digits.Length != 16 || !digits.All(char.IsDigit))
            ModelState.AddModelError("cardNumber", "Broj kartice mora imati točno 16 znamenki.");

        if (!TryParseExpiry(cardExpiry, out _))
            ModelState.AddModelError("cardExpiry", "Neispravan ili istekao datum isteka (format MM/GG).");

        var cvv = (cardCvv ?? "").Trim();
        if (cvv.Length != 3 || !cvv.All(char.IsDigit))
            ModelState.AddModelError("cardCvv", "CVV mora imati točno 3 znamenke.");

        if (!ModelState.IsValid)
            return View("Index", cart);

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var order = new Order
        {
            UserId = userIdClaim != null ? int.Parse(userIdClaim) : null,
            CustomerName = customerName,
            CustomerEmail = customerEmail,
            Address = address,
            City = city,
            TotalAmount = cart.Sum(c => c.Price * c.Quantity),
            Items = cart.Select(c => new OrderItem
            {
                ProductVariantId = c.VariantId,
                ProductName = c.ProductName,
                VariantTitle = c.VariantTitle,
                Price = c.Price,
                Quantity = c.Quantity
            }).ToList()
        };

        db.Orders.Add(order);
        await db.SaveChangesAsync();

        HttpContext.Session.Remove(CartKey);

        return RedirectToAction(nameof(Confirmation), new { id = order.Id });
    }

    private static bool TryParseExpiry(string? input, out DateTime expiry)
    {
        expiry = default;
        if (string.IsNullOrWhiteSpace(input)) return false;
        if (!DateTime.TryParseExact(input.Trim(), "MM/yy", CultureInfo.InvariantCulture, DateTimeStyles.None, out expiry))
            return false;
        var lastDay = new DateTime(expiry.Year, expiry.Month, DateTime.DaysInMonth(expiry.Year, expiry.Month));
        return lastDay >= DateTime.Today;
    }

    public async Task<IActionResult> Confirmation(int id)
    {
        var order = await db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null) return NotFound();
        return View(order);
    }

    public async Task<IActionResult> Invoice(int id)
    {
        var order = await db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null) return NotFound();

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (order.UserId.HasValue && userIdClaim != order.UserId.ToString() && !User.IsInRole("Admin"))
            return Forbid();

        return View(order);
    }
}
