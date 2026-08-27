using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace KickoffShop.Models;

public class ProductEditViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Naziv je obavezan.")]
    public string Title { get; set; } = "";

    public string Handle { get; set; } = "";
    public string Description { get; set; } = "";
    public string ProductType { get; set; } = "";
    public string ImageUrl { get; set; } = "";
    public IFormFile? ImageFile { get; set; }

    public List<VariantEditViewModel> Variants { get; set; } = new();
}

public class VariantEditViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Naziv varijante je obavezan.")]
    public string Title { get; set; } = "";

    [Range(0.01, 100000, ErrorMessage = "Cijena mora biti pozitivna.")]
    public decimal Price { get; set; }

    public decimal? CompareAtPrice { get; set; }

    [Range(0, int.MaxValue)]
    public int Stock { get; set; }

    public bool Delete { get; set; }
}
