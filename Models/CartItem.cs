namespace KickoffShop.Models;

public class CartItem
{
    public int VariantId { get; set; }
    public string ProductName { get; set; } = "";
    public string VariantTitle { get; set; } = "";
    public decimal Price { get; set; }
    public string ImageUrl { get; set; } = "";
    public int Quantity { get; set; }
    public int ProductId { get; set; }
    public string Handle { get; set; } = "";
}
