namespace KickoffShop.Models;

public class Order
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string CustomerName { get; set; } = "";
    public string CustomerEmail { get; set; } = "";
    public string Address { get; set; } = "";
    public string City { get; set; } = "";
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Primljeno";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
