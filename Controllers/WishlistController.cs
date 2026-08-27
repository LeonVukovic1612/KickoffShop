using System.Security.Claims;
using KickoffShop.Data;
using KickoffShop.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KickoffShop.Controllers;

[Authorize]
public class WishlistController(AppDbContext db) : Controller
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int productId, string? returnUrl)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var existing = await db.WishlistItems
            .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);

        if (existing is not null)
            db.WishlistItems.Remove(existing);
        else
            db.WishlistItems.Add(new WishlistItem { UserId = userId, ProductId = productId });

        await db.SaveChangesAsync();

        return Redirect(returnUrl ?? "/");
    }
}
