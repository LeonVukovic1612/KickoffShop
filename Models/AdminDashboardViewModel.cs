namespace KickoffShop.Models;

public class AdminDashboardViewModel
{
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public int MonthOrders { get; set; }
    public decimal MonthRevenue { get; set; }
    public List<Order> RecentOrders { get; set; } = new();
    public List<TopProduct> TopProducts { get; set; } = new();
    public List<DailyRevenue> RevenueChart { get; set; } = new();
    public List<ProductVariant> LowStock { get; set; } = new();
}

public record TopProduct(string Name, int Quantity, decimal Revenue);
public record DailyRevenue(string Date, decimal Revenue);
