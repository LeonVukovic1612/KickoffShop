using KickoffShop.Data;
using KickoffShop.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KickoffShop.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController(AppDbContext db, IWebHostEnvironment env) : Controller
{
    public async Task<IActionResult> Index()
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var thirtyDaysAgo = now.Date.AddDays(-29);

        var orders = await db.Orders.Include(o => o.Items).ToListAsync();

        var chartData = Enumerable.Range(0, 30)
            .Select(i => thirtyDaysAgo.AddDays(i))
            .Select(d => new DailyRevenue(
                d.ToString("dd.MM."),
                orders.Where(o => o.CreatedAt.Date == d).Sum(o => o.TotalAmount)))
            .ToList();

        var topProducts = orders
            .SelectMany(o => o.Items)
            .GroupBy(i => i.ProductName)
            .Select(g => new TopProduct(g.Key, g.Sum(i => i.Quantity), g.Sum(i => i.Price * i.Quantity)))
            .OrderByDescending(p => p.Revenue)
            .Take(5)
            .ToList();

        var lowStock = await db.ProductVariants
            .Include(v => v.Product)
            .Where(v => v.Stock < 5)
            .OrderBy(v => v.Stock)
            .ToListAsync();

        var vm = new AdminDashboardViewModel
        {
            TotalOrders = orders.Count,
            TotalRevenue = orders.Sum(o => o.TotalAmount),
            MonthOrders = orders.Count(o => o.CreatedAt >= monthStart),
            MonthRevenue = orders.Where(o => o.CreatedAt >= monthStart).Sum(o => o.TotalAmount),
            RecentOrders = orders.OrderByDescending(o => o.CreatedAt).Take(10).ToList(),
            TopProducts = topProducts,
            RevenueChart = chartData,
            LowStock = lowStock
        };

        return View(vm);
    }

    public async Task<IActionResult> Products()
    {
        var products = await db.Products.Include(p => p.Variants).OrderBy(p => p.Title).ToListAsync();
        return View(products);
    }

    [HttpGet]
    public async Task<IActionResult> EditProduct(int? id)
    {
        if (id is null)
            return View(new ProductEditViewModel { Variants = [new VariantEditViewModel()] });

        var product = await db.Products.Include(p => p.Variants).FirstOrDefaultAsync(p => p.Id == id);
        if (product is null) return NotFound();

        var vm = new ProductEditViewModel
        {
            Id = product.Id,
            Title = product.Title,
            Handle = product.Handle,
            Description = product.Description,
            ProductType = product.ProductType,
            ImageUrl = product.ImageUrl,
            Variants = product.Variants.Select(v => new VariantEditViewModel
            {
                Id = v.Id,
                Title = v.Title,
                Price = v.Price,
                CompareAtPrice = v.CompareAtPrice,
                Stock = v.Stock
            }).ToList()
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProduct(ProductEditViewModel vm)
    {
        // Process image upload before ModelState check so ImageUrl is populated
        if (vm.ImageFile is { Length: > 0 })
        {
            var uploaded = await SaveImageAsync(vm.ImageFile);
            if (uploaded is null)
                ModelState.AddModelError("ImageFile", "Neispravna slika. Dozvoljeni formati: JPG, PNG, WebP, GIF (max 5 MB).");
            else
            {
                vm.ImageUrl = uploaded;
                ModelState.Remove(nameof(vm.ImageUrl));
            }
        }

        if (vm.Id == 0 && string.IsNullOrWhiteSpace(vm.ImageUrl))
            ModelState.AddModelError("ImageFile", "Slika je obavezna za novi proizvod.");

        vm.Variants.RemoveAll(v => v.Delete && v.Id == 0);

        var activeVariants = vm.Variants.Where(v => !v.Delete).ToList();
        if (!activeVariants.Any())
            ModelState.AddModelError("Variants", "Proizvod mora imati najmanje jednu varijantu.");

        if (!ModelState.IsValid) return View(vm);

        if (string.IsNullOrWhiteSpace(vm.Handle))
            vm.Handle = Slugify(vm.Title);

        if (vm.Id == 0)
        {
            var product = new Product
            {
                Title = vm.Title,
                Handle = vm.Handle,
                Description = vm.Description,
                ProductType = vm.ProductType,
                ImageUrl = vm.ImageUrl,
                Variants = activeVariants.Select(v => new ProductVariant
                {
                    Title = v.Title,
                    Price = v.Price,
                    CompareAtPrice = v.CompareAtPrice > 0 ? v.CompareAtPrice : null,
                    Stock = v.Stock
                }).ToList()
            };
            db.Products.Add(product);
        }
        else
        {
            var product = await db.Products.Include(p => p.Variants).FirstOrDefaultAsync(p => p.Id == vm.Id);
            if (product is null) return NotFound();

            product.Title = vm.Title;
            product.Handle = vm.Handle;
            product.Description = vm.Description;
            product.ProductType = vm.ProductType;
            product.ImageUrl = vm.ImageUrl;

            var toDelete = product.Variants
                .Where(v => vm.Variants.Any(vv => vv.Id == v.Id && vv.Delete))
                .ToList();
            db.ProductVariants.RemoveRange(toDelete);

            foreach (var vvm in activeVariants)
            {
                if (vvm.Id == 0)
                {
                    product.Variants.Add(new ProductVariant
                    {
                        Title = vvm.Title,
                        Price = vvm.Price,
                        CompareAtPrice = vvm.CompareAtPrice > 0 ? vvm.CompareAtPrice : null,
                        Stock = vvm.Stock
                    });
                }
                else
                {
                    var existing = product.Variants.FirstOrDefault(v => v.Id == vvm.Id);
                    if (existing is not null)
                    {
                        existing.Title = vvm.Title;
                        existing.Price = vvm.Price;
                        existing.CompareAtPrice = vvm.CompareAtPrice > 0 ? vvm.CompareAtPrice : null;
                        existing.Stock = vvm.Stock;
                    }
                }
            }
        }

        await db.SaveChangesAsync();
        TempData["Success"] = vm.Id == 0 ? "Proizvod dodan." : "Proizvod ažuriran.";
        return RedirectToAction(nameof(Products));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await db.Products.Include(p => p.Variants).FirstOrDefaultAsync(p => p.Id == id);
        if (product is null) return NotFound();

        db.ProductVariants.RemoveRange(product.Variants);
        db.Products.Remove(product);
        await db.SaveChangesAsync();

        TempData["Success"] = "Proizvod obrisan.";
        return RedirectToAction(nameof(Products));
    }

    public async Task<IActionResult> Orders()
    {
        var orders = await db.Orders
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
        return View(orders);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateOrderStatus(int id, string status)
    {
        var order = await db.Orders.FindAsync(id);
        if (order is null) return NotFound();
        order.Status = status;
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Orders));
    }

    private static readonly string[] AllowedImageTypes = ["image/jpeg", "image/png", "image/webp", "image/gif"];
    private const long MaxImageBytes = 5 * 1024 * 1024; // 5 MB

    private async Task<string?> SaveImageAsync(IFormFile file)
    {
        if (file.Length > MaxImageBytes) return null;
        if (!AllowedImageTypes.Contains(file.ContentType)) return null;

        var folder = Path.Combine(env.WebRootPath, "uploads", "products");
        Directory.CreateDirectory(folder);

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var name = Guid.NewGuid().ToString("N") + ext;

        await using var stream = new FileStream(Path.Combine(folder, name), FileMode.Create);
        await file.CopyToAsync(stream);

        return "/uploads/products/" + name;
    }

    private static string Slugify(string text) =>
        System.Text.RegularExpressions.Regex.Replace(
            text.ToLowerInvariant().Replace(" ", "-"),
            @"[^a-z0-9\-]", "");
}
