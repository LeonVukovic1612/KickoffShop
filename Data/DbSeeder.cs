using KickoffShop.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace KickoffShop.Data;

public static class DbSeeder
{
    public static void Seed(AppDbContext db, IConfiguration config)
    {
        db.Database.EnsureCreated();

        // Add columns introduced after initial schema creation
        try { db.Database.ExecuteSqlRaw("ALTER TABLE Users ADD COLUMN Address TEXT NOT NULL DEFAULT ''"); } catch { }
        try { db.Database.ExecuteSqlRaw("ALTER TABLE Users ADD COLUMN City TEXT NOT NULL DEFAULT ''"); } catch { }

        db.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS WishlistItems (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                UserId INTEGER NOT NULL,
                ProductId INTEGER NOT NULL,
                CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),
                FOREIGN KEY(UserId) REFERENCES Users(Id) ON DELETE CASCADE,
                FOREIGN KEY(ProductId) REFERENCES Products(Id) ON DELETE CASCADE,
                UNIQUE(UserId, ProductId)
            )");

        if (!db.Users.Any())
        {
            var email = config["AdminSeed:Email"] ?? throw new InvalidOperationException("AdminSeed:Email nije postavljen u konfiguraciji.");
            var password = config["AdminSeed:Password"] ?? throw new InvalidOperationException("AdminSeed:Password nije postavljen u konfiguraciji.");

            db.Users.Add(new User
            {
                Email = email,
                Name = "Administrator",
                PasswordHash = PasswordHasher.Hash(password),
                Role = "Admin"
            });
            db.SaveChanges();
        }

        if (db.Products.Any()) return;

        var products = new List<Product>
        {
            new()
            {
                Title = "Nike Strike Football",
                Handle = "nike-strike-football",
                Description = "Lopta za trening i rekreativnu igru. Trajni materijal otporan na habanje, odlična kontrola leta.",
                ProductType = "Lopte",
                ImageUrl = "https://images.unsplash.com/photo-1614632537197-38a17061c2bd?w=600&q=80",
                Variants = new List<ProductVariant>
                {
                    new() { Title = "Veličina 4", Price = 24.99m, CompareAtPrice = 34.99m, Stock = 20 },
                    new() { Title = "Veličina 5", Price = 29.99m, CompareAtPrice = 39.99m, Stock = 15 },
                }
            },
            new()
            {
                Title = "Adidas Copa Pure.3 Kopačke",
                Handle = "adidas-copa-pure-3",
                Description = "Klasične kopačke s kožnom potplatom. Izvrsno prianjanje na travnatim terenima, udobne za dulje igranje.",
                ProductType = "Kopačke",
                ImageUrl = "https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=600&q=80",
                Variants = new List<ProductVariant>
                {
                    new() { Title = "EU 40", Price = 69.99m, Stock = 8 },
                    new() { Title = "EU 41", Price = 69.99m, Stock = 12 },
                    new() { Title = "EU 42", Price = 69.99m, Stock = 10 },
                    new() { Title = "EU 43", Price = 69.99m, Stock = 7 },
                    new() { Title = "EU 44", Price = 69.99m, Stock = 5 },
                }
            },
            new()
            {
                Title = "Croatia 2024 Dres (Domaći)",
                Handle = "croatia-2024-home-jersey",
                Description = "Replika domaćeg dresa reprezentacije Hrvatske za sezonu 2024. Lagani poliesterski materijal, kockasti uzorak.",
                ProductType = "Dresovi",
                ImageUrl = "https://images.unsplash.com/photo-1431324155629-1a6deb1dec8d?w=600&q=80",
                Variants = new List<ProductVariant>
                {
                    new() { Title = "S", Price = 59.99m, CompareAtPrice = 79.99m, Stock = 15 },
                    new() { Title = "M", Price = 59.99m, CompareAtPrice = 79.99m, Stock = 20 },
                    new() { Title = "L", Price = 59.99m, CompareAtPrice = 79.99m, Stock = 18 },
                    new() { Title = "XL", Price = 59.99m, CompareAtPrice = 79.99m, Stock = 10 },
                }
            },
            new()
            {
                Title = "Puma King Platinum Kopačke",
                Handle = "puma-king-platinum",
                Description = "Premijum kopačke od klokan kože. Dizajnirane za vrhunsku kontrolu lopte, nosili su ih legende.",
                ProductType = "Kopačke",
                ImageUrl = "https://images.unsplash.com/photo-1600185365926-3a2ce3cdb9eb?w=600&q=80",
                Variants = new List<ProductVariant>
                {
                    new() { Title = "EU 40", Price = 119.99m, Stock = 3 },
                    new() { Title = "EU 41", Price = 119.99m, Stock = 4 },
                    new() { Title = "EU 42", Price = 119.99m, Stock = 6 },
                    new() { Title = "EU 43", Price = 119.99m, Stock = 5 },
                    new() { Title = "EU 44", Price = 119.99m, Stock = 2 },
                }
            },
            new()
            {
                Title = "Real Madrid 2024/25 Dres (Gostujući)",
                Handle = "real-madrid-2024-away",
                Description = "Službeni gostujući dres Real Madrida za sezonu 2024/25. Dri-FIT tehnologija, udoban za igru.",
                ProductType = "Dresovi",
                ImageUrl = "https://images.unsplash.com/photo-1517466787929-bc90951d0974?w=600&q=80",
                Variants = new List<ProductVariant>
                {
                    new() { Title = "S", Price = 89.99m, Stock = 10 },
                    new() { Title = "M", Price = 89.99m, Stock = 14 },
                    new() { Title = "L", Price = 89.99m, Stock = 11 },
                    new() { Title = "XL", Price = 89.99m, Stock = 6 },
                }
            },
            new()
            {
                Title = "Nike Shin Guards Strike",
                Handle = "nike-shin-guards-strike",
                Description = "Lagani štitnici za potkoljenicu. Anatomski oblik za bolji fit, soft podloga za udobnost.",
                ProductType = "Štitnici",
                ImageUrl = "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b?w=600&q=80",
                Variants = new List<ProductVariant>
                {
                    new() { Title = "S/M", Price = 14.99m, CompareAtPrice = 19.99m, Stock = 30 },
                    new() { Title = "L/XL", Price = 14.99m, CompareAtPrice = 19.99m, Stock = 25 },
                }
            },
            new()
            {
                Title = "Adidas Tiro 23 Trening Hlače",
                Handle = "adidas-tiro-23-pants",
                Description = "Klasične trening hlače za teren i teretanu. Lagane, prozračne, s džepovima sa zatvaračem.",
                ProductType = "Oprema",
                ImageUrl = "https://images.unsplash.com/photo-1571902943202-507ec2618e8f?w=600&q=80",
                Variants = new List<ProductVariant>
                {
                    new() { Title = "S", Price = 34.99m, Stock = 20 },
                    new() { Title = "M", Price = 34.99m, Stock = 25 },
                    new() { Title = "L", Price = 34.99m, Stock = 18 },
                    new() { Title = "XL", Price = 34.99m, Stock = 12 },
                }
            },
            new()
            {
                Title = "Puma Orbita 6 MS Liga Lopta",
                Handle = "puma-orbita-6",
                Description = "Službena lopta za ligu. Termospajana konstrukcija, savršena sferičnost za predvidivu putanju.",
                ProductType = "Lopte",
                ImageUrl = "https://images.unsplash.com/photo-1551698618-1dfe5d97d256?w=600&q=80",
                Variants = new List<ProductVariant>
                {
                    new() { Title = "Veličina 5", Price = 49.99m, CompareAtPrice = 64.99m, Stock = 10 },
                }
            },
        };

        db.Products.AddRange(products);
        db.SaveChanges();
    }
}
