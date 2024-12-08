namespace EventPlanner.Models;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public UserRole Role { get; set; }
    public bool IsEmailConfirmed { get; set; } 
    public string? EmailConfirmationToken { get; set; } 
    public string FullName { get; set; } 
    public string PhoneNumber { get; set; } 
    public string Address { get; set; } 
    public  List<Event> Events { get; set; } 
}