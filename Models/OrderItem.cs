namespace KickoffShop.Models;

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductVariantId { get; set; }
    public string ProductName { get; set; } = "";
    public string VariantTitle { get; set; } = "";
    public decimal Price { get; set; }
    public int Quantity { get; set; }

    public Order Order { get; set; } = null!;
}
