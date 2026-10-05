namespace LlmBasics.Tickets;

public enum TicketPriority
{
    Low,
    Medium,
    High
}

public enum TicketStatus
{
    Open,
    Closed
}

public sealed record Ticket(int Id, string Title, TicketPriority Priority, TicketStatus Status);
