using System.Text.Json;
using KickoffShop.Data;
using KickoffShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KickoffShop.Controllers;

public class CartController(AppDbContext db) : Controller
{
    private const string CartKey = "Cart";

    private List<CartItem> GetCart()
    {
        var json = HttpContext.Session.GetString(CartKey);
        return json is null ? [] : JsonSerializer.Deserialize<List<CartItem>>(json)!;
    }

    private void SaveCart(List<CartItem> cart)
    {
        HttpContext.Session.SetString(CartKey, JsonSerializer.Serialize(cart));
    }

    public IActionResult Index()
    {
        return View(GetCart());
    }

    [HttpPost]
    public async Task<IActionResult> Add(int variantId, int quantity = 1)
    {
        var variant = await db.ProductVariants
            .Include(v => v.Product)
            .FirstOrDefaultAsync(v => v.Id == variantId);

        if (variant is null) return NotFound();

        var cart = GetCart();
        var existing = cart.FirstOrDefault(c => c.VariantId == variantId);

        if (existing is not null)
            existing.Quantity += quantity;
        else
            cart.Add(new CartItem
            {
                VariantId = variantId,
                ProductId = variant.ProductId,
                Handle = variant.Product.Handle,
                ProductName = variant.Product.Title,
                VariantTitle = variant.Title,
                Price = variant.Price,
                ImageUrl = variant.Product.ImageUrl,
                Quantity = quantity
            });

        SaveCart(cart);
        TempData["Success"] = $"{variant.Product.Title} ({variant.Title}) dodan u košaricu.";
        return RedirectToAction("Details", "Product", new { id = variant.Product.Handle });
    }

    [HttpPost]
    public IActionResult Update(int variantId, int quantity)
    {
        var cart = GetCart();
        var item = cart.FirstOrDefault(c => c.VariantId == variantId);
        if (item is not null)
        {
            if (quantity <= 0) cart.Remove(item);
            else item.Quantity = quantity;
        }
        SaveCart(cart);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult Remove(int variantId)
    {
        var cart = GetCart();
        cart.RemoveAll(c => c.VariantId == variantId);
        SaveCart(cart);
        return RedirectToAction(nameof(Index));
    }
}
