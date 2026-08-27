namespace KickoffShop.Models;

public class Product
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Handle { get; set; } = "";
    public string Description { get; set; } = "";
    public string ProductType { get; set; } = "";
    public string ImageUrl { get; set; } = "";

    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
}
