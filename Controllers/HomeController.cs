using KickoffShop.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KickoffShop.Controllers;

public class HomeController(AppDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var products = await db.Products.Include(p => p.Variants).ToListAsync();

        var onSale = await db.Products
            .Include(p => p.Variants)
            .Where(p => p.Variants.Any(v => v.CompareAtPrice != null && v.CompareAtPrice > v.Price))
            .ToListAsync();

        ViewBag.OnSale = onSale;
        return View(products);
    }
}
