namespace LlmBasics.Orders;

public enum OrderStatus
{
    Pending,
    Paid,
    Shipped,
    Cancelled
}

public sealed record OrderLine(string ProductName, int Quantity, decimal UnitPrice)
{
    public decimal LineTotal => Quantity * UnitPrice;
}

public sealed record Order(int Id, string CustomerName, IReadOnlyList<OrderLine> Lines, OrderStatus Status)
{
    public decimal Total => Lines.Sum(l => l.LineTotal);
}
