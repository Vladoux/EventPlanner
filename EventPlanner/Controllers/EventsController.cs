using System.Security.Claims;
using AutoMapper;
using EventPlanner.Models;
using EventPlanner.Models.FrontModels;
using EventPlanner.Repositories;
using EventPlanner.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventPlanner.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController(IRepository<Event> eventRepository,IRepository<User> userRepository,EmailService emailService, IMapper mapper) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllEvents()
    { 
        var events = await eventRepository.GetAllAsync(e => e.Attributes); // Подгружаем атрибуты
        var eventDtos = mapper.Map<IEnumerable<EventDto>>(events); // Маппим в DTO
        return Ok(eventDtos);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateEvent([FromBody] CreateEventDto eventDtoData)
    {
        var newEvent = mapper.Map<Event>(eventDtoData);
        await eventRepository.AddAsync(newEvent);

        return CreatedAtAction(nameof(GetAllEvents), new { id = newEvent.Id }, eventDtoData);
    }
    
    [Authorize(Roles = "User")]
    [HttpGet("{eventId}/register")]
    public async Task<IActionResult> RegisterEvent([FromHeader] string eventId)
    {
        var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
            return Unauthorized(new { Message = "User email not found in token." });
        
        // Получаем событие
        var myEvent = await eventRepository.GetByIdAsync(Guid.Parse(eventId));
        if (myEvent == null)
            return NotFound(new { Message = "Event not found." });

        // Получаем пользователя
        var user = (await userRepository.GetAllAsync(x=>x.Email == email)).FirstOrDefault();
        if (user == null)
            return NotFound(new { Message = "User not found." });

        // Проверяем лимит участников
        if (myEvent.Participants.Count >= myEvent.MaxParticipants)
            return BadRequest(new { Message = "Event participant limit reached." });

        // Проверяем, зарегистрирован ли пользователь
        if (myEvent.Participants.Any(u => u.Id == user.Id))
            return BadRequest(new { Message = "User is already registered for this event." });

        // Добавляем пользователя к событию
        myEvent.Participants.Add(user);
        await eventRepository.UpdateAsync(myEvent);

        // Отправляем подтверждение на email
        await emailService.SendEmailAsync(user.Email, "Registration Confirmation",
            $"Dear {user.Email},\n\nYou have successfully registered for the event '{myEvent.Name}'.");

        return Ok(new { Message = "User successfully registered for the event." });
    }
    
}