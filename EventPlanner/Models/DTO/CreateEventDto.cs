namespace EventPlanner.Models.FrontModels;

[Serializable]
public class CreateEventDto
{
    public string Name { get; set; }
    public int MaxParticipants { get; set; }
    public List<CreateEventAttributeDto> Attributes { get; set; } 
}