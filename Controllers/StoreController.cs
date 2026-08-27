using KickoffShop.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KickoffShop.Controllers;

public class StoreController(AppDbContext db) : Controller
{
    public async Task<IActionResult> Index(string? q, string? category, string? sort)
    {
        var query = db.Products.Include(p => p.Variants).AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var pattern = $"%{q}%";
            query = query.Where(p => EF.Functions.Like(p.Title, pattern) || EF.Functions.Like(p.ProductType, pattern));
        }

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(p => p.ProductType == category);

        var products = await query.ToListAsync();

        var sorted = sort switch
        {
            "price_asc"  => products.OrderBy(p => p.Variants.Min(v => v.Price)).ToList(),
            "price_desc" => products.OrderByDescending(p => p.Variants.Min(v => v.Price)).ToList(),
            _            => products
        };

        ViewBag.Categories = await db.Products
            .Select(p => p.ProductType)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        ViewBag.CurrentCategory = category;
        ViewBag.CurrentSort     = sort;
        ViewBag.SearchQuery     = q?.Trim();

        return View(sorted);
    }
}
