using System.Text.Json;
using KickoffShop.Models;
using Microsoft.AspNetCore.Mvc;

namespace KickoffShop.ViewComponents;

public class CartBadgeViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        var json = HttpContext.Session.GetString("Cart");
        var count = 0;
        if (json is not null)
        {
            var cart = JsonSerializer.Deserialize<List<CartItem>>(json);
            count = cart?.Sum(i => i.Quantity) ?? 0;
        }
        return View(count);
    }
}
