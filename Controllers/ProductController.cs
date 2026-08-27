using System.Security.Claims;
using KickoffShop.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KickoffShop.Controllers;

public class ProductController(AppDbContext db) : Controller
{
    public async Task<IActionResult> Details(string id)
    {
        var product = await db.Products
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Handle == id);

        if (product is null) return NotFound();

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim != null)
        {
            var userId = int.Parse(userIdClaim);
            ViewBag.IsWishlisted = await db.WishlistItems
                .AnyAsync(w => w.UserId == userId && w.ProductId == product.Id);
        }
        else
        {
            ViewBag.IsWishlisted = false;
        }

        ViewBag.Related = await db.Products
            .Include(p => p.Variants)
            .Where(p => p.ProductType == product.ProductType && p.Id != product.Id)
            .Take(4)
            .ToListAsync();

        return View(product);
    }
}
