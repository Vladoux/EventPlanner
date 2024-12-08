namespace EventPlanner.Models.FrontModels;

[Serializable]
public class EventDto
{
    public Guid Id { get; set; } 
    public string Name { get; set; }
    public int MaxParticipants { get; set; }
    public List<EventAttributeDto> Attributes { get; set; } 
}