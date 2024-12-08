namespace EventPlanner.Models.FrontModels;

[Serializable]
public class CreateEventAttributeDto
{
    public string Name { get; set; }
    public string Value { get; set; }
}