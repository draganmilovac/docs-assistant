using LlmBasics.Orders;

namespace LlmBasics.Tests;

public class OrderServiceTests
{
    private readonly OrderService _service = new();

    private static OrderLine Line(string product = "Tastatura", int quantity = 1, decimal unitPrice = 10m) =>
        new(product, quantity, unitPrice);

    private Order CreateOrder(string customer = "Marko") => _service.Create(customer, [Line()]);

    // --- Create ---

    [Fact]
    public void Create_ReturnsPendingOrderWithGivenData()
    {
        var lines = new[] { Line("Miš", 2, 15m), Line("Monitor", 1, 200m) };

        var order = _service.Create("Ana", lines);

        Assert.Equal("Ana", order.CustomerName);
        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.Equal(lines, order.Lines);
    }

    [Fact]
    public void Create_AssignsIncrementingIdsStartingAtOne()
    {
        var first = CreateOrder();
        var second = CreateOrder();

        Assert.Equal(1, first.Id);
        Assert.Equal(2, second.Id);
    }

    [Fact]
    public void Create_TrimsCustomerName()
    {
        var order = _service.Create("  Ana  ", [Line()]);

        Assert.Equal("Ana", order.CustomerName);
    }

    [Fact]
    public void Create_CopiesLines_SoCallerChangesDoNotAffectOrder()
    {
        var lines = new List<OrderLine> { Line("A", 1, 10m) };

        var order = _service.Create("Ana", lines);
        lines.Add(Line("B", 1, 999m));

        Assert.Single(order.Lines);
        Assert.Equal(10m, order.Total);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyCustomerName_Throws(string? customer)
    {
        var ex = Assert.Throws<ArgumentException>(() => _service.Create(customer!, [Line()]));
        Assert.Equal("customerName", ex.ParamName);
    }

    [Fact]
    public void Create_WithNullLines_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => _service.Create("Ana", null!));
    }

    [Fact]
    public void Create_WithNoLines_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() => _service.Create("Ana", []));
        Assert.Equal("lines", ex.ParamName);
    }

    [Fact]
    public void Create_WithNullLine_Throws()
    {
        Assert.Throws<ArgumentException>(() => _service.Create("Ana", [Line(), null!]));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_WithEmptyProductName_Throws(string product)
    {
        Assert.Throws<ArgumentException>(() => _service.Create("Ana", [Line(product: product)]));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void Create_WithNonPositiveQuantity_Throws(int quantity)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _service.Create("Ana", [Line(quantity: quantity)]));
    }

    [Fact]
    public void Create_WithNegativeUnitPrice_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _service.Create("Ana", [Line(unitPrice: -0.01m)]));
    }

    [Fact]
    public void Create_WithZeroUnitPrice_IsAllowed()
    {
        var order = _service.Create("Ana", [Line(unitPrice: 0m)]);

        Assert.Equal(0m, order.Total);
    }

    [Fact]
    public void Create_InvalidInput_DoesNotStoreOrderOrConsumeId()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _service.Create("Ana", [Line(), Line(quantity: 0)]));

        var order = CreateOrder();

        Assert.Equal(1, order.Id);
        Assert.Single(_service.GetByStatus(OrderStatus.Pending));
    }

    // --- Total ---

    [Fact]
    public void Total_IsSumOfQuantityTimesUnitPrice()
    {
        var order = _service.Create("Ana", [Line("A", 3, 2.50m), Line("B", 2, 10.10m)]);

        Assert.Equal(27.70m, order.Total);
    }

    // --- Status transitions ---

    [Fact]
    public void Pay_PendingOrder_BecomesPaid()
    {
        var order = CreateOrder();

        var paid = _service.Pay(order.Id);

        Assert.Equal(OrderStatus.Paid, paid.Status);
        Assert.Equal(OrderStatus.Paid, _service.GetById(order.Id)!.Status);
    }

    [Fact]
    public void Ship_PaidOrder_BecomesShipped()
    {
        var order = CreateOrder();
        _service.Pay(order.Id);

        var shipped = _service.Ship(order.Id);

        Assert.Equal(OrderStatus.Shipped, shipped.Status);
    }

    [Fact]
    public void Cancel_PendingOrder_BecomesCancelled()
    {
        var order = CreateOrder();

        Assert.Equal(OrderStatus.Cancelled, _service.Cancel(order.Id).Status);
    }

    [Fact]
    public void Cancel_PaidOrder_BecomesCancelled()
    {
        var order = CreateOrder();
        _service.Pay(order.Id);

        Assert.Equal(OrderStatus.Cancelled, _service.Cancel(order.Id).Status);
    }

    [Fact]
    public void Transition_KeepsOtherOrderData()
    {
        var order = CreateOrder("Ana");

        var paid = _service.Pay(order.Id);

        Assert.Equal(order with { Status = OrderStatus.Paid }, paid);
    }

    [Fact]
    public void Ship_PendingOrder_Throws()
    {
        var order = CreateOrder();

        Assert.Throws<InvalidOperationException>(() => _service.Ship(order.Id));
        Assert.Equal(OrderStatus.Pending, _service.GetById(order.Id)!.Status);
    }

    [Fact]
    public void Pay_AlreadyPaidOrder_Throws()
    {
        var order = CreateOrder();
        _service.Pay(order.Id);

        Assert.Throws<InvalidOperationException>(() => _service.Pay(order.Id));
    }

    [Fact]
    public void Cancel_ShippedOrder_Throws()
    {
        var order = CreateOrder();
        _service.Pay(order.Id);
        _service.Ship(order.Id);

        Assert.Throws<InvalidOperationException>(() => _service.Cancel(order.Id));
        Assert.Equal(OrderStatus.Shipped, _service.GetById(order.Id)!.Status);
    }

    [Fact]
    public void CancelledOrder_CannotBePaidShippedOrCancelledAgain()
    {
        var order = CreateOrder();
        _service.Cancel(order.Id);

        Assert.Throws<InvalidOperationException>(() => _service.Pay(order.Id));
        Assert.Throws<InvalidOperationException>(() => _service.Ship(order.Id));
        Assert.Throws<InvalidOperationException>(() => _service.Cancel(order.Id));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(99)]
    public void Transitions_WithUnknownId_ThrowKeyNotFound(int id)
    {
        CreateOrder();

        Assert.Throws<KeyNotFoundException>(() => _service.Pay(id));
        Assert.Throws<KeyNotFoundException>(() => _service.Ship(id));
        Assert.Throws<KeyNotFoundException>(() => _service.Cancel(id));
    }

    // --- Queries ---

    [Fact]
    public void GetById_UnknownId_ReturnsNull()
    {
        Assert.Null(_service.GetById(1));
    }

    [Fact]
    public void GetByStatus_ReturnsOnlyMatchingOrdersInInsertionOrder()
    {
        var a = CreateOrder();
        var b = CreateOrder();
        var c = CreateOrder();
        _service.Pay(b.Id);

        Assert.Equal([a, c], _service.GetByStatus(OrderStatus.Pending));
        Assert.Equal([b.Id], _service.GetByStatus(OrderStatus.Paid).Select(o => o.Id));
        Assert.Empty(_service.GetByStatus(OrderStatus.Shipped));
    }

    [Fact]
    public void GetByStatus_ReturnsSnapshotUnaffectedByLaterChanges()
    {
        var a = CreateOrder();
        var snapshot = _service.GetByStatus(OrderStatus.Pending);

        _service.Pay(a.Id);
        CreateOrder();

        Assert.Equal([a], snapshot);
    }

    [Fact]
    public void GetByCustomer_IsCaseInsensitiveAndIgnoresSurroundingWhitespace()
    {
        var ana1 = CreateOrder("Ana");
        CreateOrder("Marko");
        var ana2 = CreateOrder("ANA");

        Assert.Equal([ana1, ana2], _service.GetByCustomer("  ana "));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Nepoznat")]
    public void GetByCustomer_WithNoMatch_ReturnsEmpty(string? customer)
    {
        CreateOrder("Ana");

        Assert.Empty(_service.GetByCustomer(customer!));
    }

    [Fact]
    public void GetTotalRevenue_OnEmptyService_IsZero()
    {
        Assert.Equal(0m, _service.GetTotalRevenue());
    }

    [Fact]
    public void GetTotalRevenue_CountsOnlyPaidAndShippedOrders()
    {
        var pending = _service.Create("A", [Line(unitPrice: 1m)]);
        var paid = _service.Create("B", [Line(unitPrice: 10m)]);
        var shipped = _service.Create("C", [Line(unitPrice: 100m)]);
        var cancelled = _service.Create("D", [Line(unitPrice: 1000m)]);
        _service.Pay(paid.Id);
        _service.Pay(shipped.Id);
        _service.Ship(shipped.Id);
        _service.Pay(cancelled.Id);
        _service.Cancel(cancelled.Id);

        Assert.Equal(110m, _service.GetTotalRevenue());
    }
}
