using System.Security.Claims;
using KickoffShop.Data;
using KickoffShop.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KickoffShop.Controllers;

[Authorize]
public class ProfileController(AppDbContext db) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await db.Users.FindAsync(userId);
        if (user is null) return NotFound();

        var wishlist = await db.WishlistItems
            .Where(w => w.UserId == userId)
            .Include(w => w.Product).ThenInclude(p => p.Variants)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync();

        return View(new ProfileViewModel
        {
            Name = user.Name,
            Email = user.Email,
            Address = user.Address,
            City = user.City,
            Wishlist = wishlist
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ProfileViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await db.Users.FindAsync(userId);
        if (user is null) return NotFound();

        if (user.Email != model.Email && await db.Users.AnyAsync(u => u.Email == model.Email))
        {
            ModelState.AddModelError("Email", "Email adresa je već registrirana.");
            return View(model);
        }

        user.Name = model.Name;
        user.Email = model.Email;
        user.Address = model.Address;
        user.City = model.City;

        await db.SaveChangesAsync();

        // Refresh auth cookie so claims reflect new name/email
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        model.Saved = true;
        return View(model);
    }
}
