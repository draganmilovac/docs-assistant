namespace LlmBasics.Orders;

/// <summary>
/// Keeps orders in memory. Allowed transitions:
/// Pending -> Paid -> Shipped, and Pending/Paid -> Cancelled.
/// </summary>
public sealed class OrderService
{
    private readonly List<Order> _orders = [];
    private int _nextId = 1;

    public Order Create(string customerName, IEnumerable<OrderLine> lines)
    {
        if (string.IsNullOrWhiteSpace(customerName))
            throw new ArgumentException("Customer name must not be empty.", nameof(customerName));
        ArgumentNullException.ThrowIfNull(lines);

        // Copy so later changes to the caller's collection don't affect the order.
        var copy = lines.ToArray();
        if (copy.Length == 0)
            throw new ArgumentException("Order must have at least one line.", nameof(lines));
        foreach (var line in copy)
            Validate(line);

        var order = new Order(_nextId++, customerName.Trim(), copy, OrderStatus.Pending);
        _orders.Add(order);
        return order;
    }

    public Order Pay(int id) => Transition(id, OrderStatus.Paid, OrderStatus.Pending);

    public Order Ship(int id) => Transition(id, OrderStatus.Shipped, OrderStatus.Paid);

    public Order Cancel(int id) => Transition(id, OrderStatus.Cancelled, OrderStatus.Pending, OrderStatus.Paid);

    public Order? GetById(int id) => _orders.Find(o => o.Id == id);

    public IReadOnlyList<Order> GetByStatus(OrderStatus status) =>
        _orders.Where(o => o.Status == status).ToList();

    public IReadOnlyList<Order> GetByCustomer(string customerName) =>
        _orders
            .Where(o => string.Equals(o.CustomerName, customerName?.Trim(), StringComparison.OrdinalIgnoreCase))
            .ToList();

    /// <summary>Sum of totals for orders that have been paid (including shipped ones).</summary>
    public decimal GetTotalRevenue() =>
        _orders
            .Where(o => o.Status is OrderStatus.Paid or OrderStatus.Shipped)
            .Sum(o => o.Total);

    private Order Transition(int id, OrderStatus target, params OrderStatus[] allowedFrom)
    {
        var index = _orders.FindIndex(o => o.Id == id);
        if (index < 0)
            throw new KeyNotFoundException($"Order {id} does not exist.");

        var current = _orders[index];
        if (!allowedFrom.Contains(current.Status))
            throw new InvalidOperationException(
                $"Order {id} cannot go from {current.Status} to {target}.");

        var updated = current with { Status = target };
        _orders[index] = updated;
        return updated;
    }

    private static void Validate(OrderLine? line)
    {
        if (line is null)
            throw new ArgumentException("Order lines must not be null.", "lines");
        if (string.IsNullOrWhiteSpace(line.ProductName))
            throw new ArgumentException("Product name must not be empty.", "lines");
        if (line.Quantity <= 0)
            throw new ArgumentOutOfRangeException("lines", line.Quantity, "Quantity must be positive.");
        if (line.UnitPrice < 0)
            throw new ArgumentOutOfRangeException("lines", line.UnitPrice, "Unit price must not be negative.");
    }
}
