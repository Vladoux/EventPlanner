namespace EventPlanner.Models.FrontModels;

[Serializable]
public class EventAttributeDto
{
    public Guid Id { get; set; } 
    public string Name { get; set; }
    public string Value { get; set; }
}