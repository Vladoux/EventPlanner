namespace EventPlanner.Models;

public class EventAttribute
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; }
    public string Value { get; set; }
    public Guid EventId { get; set; }
    public Event Event { get; set; }
}