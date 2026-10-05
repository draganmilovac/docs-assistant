using LlmBasics.Tickets;

namespace LlmBasics.Tests;

public class TicketServiceTests
{
    private readonly TicketService _service = new();

    // --- Add ---

    [Fact]
    public void Add_ReturnsOpenTicketWithGivenTitleAndPriority()
    {
        var ticket = _service.Add("Login ne radi", TicketPriority.High);

        Assert.Equal("Login ne radi", ticket.Title);
        Assert.Equal(TicketPriority.High, ticket.Priority);
        Assert.Equal(TicketStatus.Open, ticket.Status);
    }

    [Fact]
    public void Add_AssignsUniqueIncrementingIdsStartingAtOne()
    {
        var first = _service.Add("A", TicketPriority.Low);
        var second = _service.Add("B", TicketPriority.Low);
        var third = _service.Add("C", TicketPriority.Low);

        Assert.Equal([1, 2, 3], new[] { first.Id, second.Id, third.Id });
    }

    [Fact]
    public void Add_AllowsDuplicateTitles()
    {
        var first = _service.Add("Isti naslov", TicketPriority.Medium);
        var second = _service.Add("Isti naslov", TicketPriority.Medium);

        Assert.NotEqual(first.Id, second.Id);
    }

    [Fact]
    public void Add_TrimsTitle()
    {
        var ticket = _service.Add("  Naslov  ", TicketPriority.Low);

        Assert.Equal("Naslov", ticket.Title);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n")]
    public void Add_WithEmptyTitle_Throws(string? title)
    {
        var ex = Assert.Throws<ArgumentException>(() => _service.Add(title!, TicketPriority.Low));
        Assert.Equal("title", ex.ParamName);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(3)]
    [InlineData(int.MaxValue)]
    public void Add_WithUndefinedPriority_Throws(int priority)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _service.Add("Naslov", (TicketPriority)priority));
    }

    [Fact]
    public void Add_InvalidInput_DoesNotConsumeId()
    {
        Assert.Throws<ArgumentException>(() => _service.Add("", TicketPriority.Low));

        var ticket = _service.Add("Validan", TicketPriority.Low);

        Assert.Equal(1, ticket.Id);
    }

    // --- Close ---

    [Fact]
    public void Close_ExistingOpenTicket_ReturnsClosedTicket()
    {
        var ticket = _service.Add("A", TicketPriority.High);

        var closed = _service.Close(ticket.Id);

        Assert.Equal(TicketStatus.Closed, closed.Status);
        Assert.Equal(ticket with { Status = TicketStatus.Closed }, closed);
    }

    [Fact]
    public void Close_RemovesTicketFromOpenSearch()
    {
        var ticket = _service.Add("A", TicketPriority.High);

        _service.Close(ticket.Id);

        Assert.Empty(_service.GetOpenByPriority(TicketPriority.High));
    }

    [Fact]
    public void Close_OnlyAffectsTicketWithGivenId()
    {
        var a = _service.Add("A", TicketPriority.High);
        var b = _service.Add("B", TicketPriority.High);

        _service.Close(a.Id);

        Assert.Equal([b], _service.GetOpenByPriority(TicketPriority.High));
    }

    [Fact]
    public void Close_AlreadyClosedTicket_IsNoOp()
    {
        var ticket = _service.Add("A", TicketPriority.Low);
        _service.Close(ticket.Id);

        var closedAgain = _service.Close(ticket.Id);

        Assert.Equal(TicketStatus.Closed, closedAgain.Status);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(2)]
    [InlineData(int.MaxValue)]
    public void Close_UnknownId_ThrowsKeyNotFound(int id)
    {
        _service.Add("A", TicketPriority.Low);

        Assert.Throws<KeyNotFoundException>(() => _service.Close(id));
    }

    [Fact]
    public void Close_OnEmptyService_ThrowsKeyNotFound()
    {
        Assert.Throws<KeyNotFoundException>(() => _service.Close(1));
    }

    // --- GetOpenByPriority ---

    [Fact]
    public void GetOpenByPriority_OnEmptyService_ReturnsEmpty()
    {
        Assert.Empty(_service.GetOpenByPriority(TicketPriority.Medium));
    }

    [Fact]
    public void GetOpenByPriority_ReturnsOnlyOpenTicketsWithMatchingPriority()
    {
        var highOpen1 = _service.Add("H1", TicketPriority.High);
        var highClosed = _service.Add("H2", TicketPriority.High);
        _service.Add("M1", TicketPriority.Medium);
        _service.Add("L1", TicketPriority.Low);
        var highOpen2 = _service.Add("H3", TicketPriority.High);
        _service.Close(highClosed.Id);

        var result = _service.GetOpenByPriority(TicketPriority.High);

        Assert.Equal([highOpen1, highOpen2], result);
    }

    [Fact]
    public void GetOpenByPriority_ReturnsTicketsInInsertionOrder()
    {
        var a = _service.Add("A", TicketPriority.Low);
        var b = _service.Add("B", TicketPriority.Low);
        var c = _service.Add("C", TicketPriority.Low);

        Assert.Equal([a, b, c], _service.GetOpenByPriority(TicketPriority.Low));
    }

    [Fact]
    public void GetOpenByPriority_WhenAllMatchingAreClosed_ReturnsEmpty()
    {
        var a = _service.Add("A", TicketPriority.Medium);
        var b = _service.Add("B", TicketPriority.Medium);
        _service.Close(a.Id);
        _service.Close(b.Id);

        Assert.Empty(_service.GetOpenByPriority(TicketPriority.Medium));
    }

    [Fact]
    public void GetOpenByPriority_ReturnsSnapshotUnaffectedByLaterChanges()
    {
        var a = _service.Add("A", TicketPriority.High);
        var snapshot = _service.GetOpenByPriority(TicketPriority.High);

        _service.Add("B", TicketPriority.High);
        _service.Close(a.Id);

        Assert.Equal([a], snapshot);
    }

    [Fact]
    public void GetOpenByPriority_WithUndefinedPriority_ReturnsEmpty()
    {
        _service.Add("A", TicketPriority.Low);

        Assert.Empty(_service.GetOpenByPriority((TicketPriority)99));
    }
}
