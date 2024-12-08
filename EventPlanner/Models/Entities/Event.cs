namespace EventPlanner.Models;

public class Event
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; }
    public int MaxParticipants { get; set; }
    public List<EventAttribute> Attributes { get; set; } 
    public List<User> Participants { get; set; }
}