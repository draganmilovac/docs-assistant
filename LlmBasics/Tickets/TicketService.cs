namespace LlmBasics.Tickets;

public sealed class TicketService
{
    private readonly List<Ticket> _tickets = [];
    private int _nextId = 1;

    public Ticket Add(string title, TicketPriority priority)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title must not be empty.", nameof(title));
        if (!Enum.IsDefined(priority))
            throw new ArgumentOutOfRangeException(nameof(priority), priority, "Unknown priority.");

        var ticket = new Ticket(_nextId++, title.Trim(), priority, TicketStatus.Open);
        _tickets.Add(ticket);
        return ticket;
    }

    /// <summary>
    /// Closes the ticket with the given id. Closing an already closed ticket is a no-op.
    /// </summary>
    /// <exception cref="KeyNotFoundException">No ticket with the given id exists.</exception>
    public Ticket Close(int id)
    {
        var index = _tickets.FindIndex(t => t.Id == id);
        if (index < 0)
            throw new KeyNotFoundException($"Ticket {id} does not exist.");

        var closed = _tickets[index] with { Status = TicketStatus.Closed };
        _tickets[index] = closed;
        return closed;
    }

    public IReadOnlyList<Ticket> GetOpenByPriority(TicketPriority priority) =>
        _tickets
            .Where(t => t.Status == TicketStatus.Open && t.Priority == priority)
            .ToList();
}
