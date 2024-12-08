using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EventPlanner.Models;
using EventPlanner.Models.FrontModels;
using EventPlanner.Repositories;
using EventPlanner.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace EventPlanner.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IRepository<User> userRepository, IConfiguration configuration, EmailService emailService)
    : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] LoginUserDto loginDto)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = loginDto.Email,
            PasswordHash = Crypt.HashPassword(loginDto.Password),
            IsEmailConfirmed = false,
            EmailConfirmationToken = Guid.NewGuid().ToString() 
        };

        await userRepository.AddAsync(user);

        var confirmationLink = Url.Action("ConfirmEmail", "Auth", new { token = user.EmailConfirmationToken }, Request.Scheme);
        await emailService.SendEmailAsync(user.Email, "Confirm your email", $"Click the link to confirm your email: {confirmationLink}");

        return Ok("Registration successful! Please confirm your email.");
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(string token)
    {
        var user = (await userRepository.GetAllAsync()).FirstOrDefault(u => u.EmailConfirmationToken == token);

        if (user == null)
            return BadRequest("Invalid token.");

        user.IsEmailConfirmed = true;
        user.EmailConfirmationToken = null;

        await userRepository.UpdateAsync(user);

        return Ok("Email confirmed successfully!");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginUserDto loginDto)
    {
        var user = (await userRepository.GetAllAsync())
            .FirstOrDefault(u => u.Email == loginDto.Email);

        if (user == null || !Crypt.Verify(loginDto.Password, user.PasswordHash))
            return Unauthorized("Invalid credentials.");

        if (!user.IsEmailConfirmed)
            return Unauthorized("Email not confirmed. Please check your inbox.");
        if (string.IsNullOrEmpty(user.FullName))
        {
            return BadRequest(new
            {
                Message = "Your profile is incomplete. Please update your profile.",
                RedirectTo = "/api/users/profile" 
            });
        }

        // Генерация JWT и возврат
        var token = GenerateJwtToken(user);
        return Ok(token);
    }
    
    [Authorize(Roles = "User")]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileDto updateDto)
    {
        
        var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
            return Unauthorized(new { Message = "User email not found in token." });

        
        var user = (await userRepository.GetAllAsync(x=>x.Email == email)).FirstOrDefault();
        if (user == null)
            return NotFound(new { Message = "User not found." });

        
        user.FullName = updateDto.FullName;
        user.PhoneNumber = updateDto.PhoneNumber;
        user.Address = updateDto.Address;

        await userRepository.UpdateAsync(user);

        return Ok(new { Message = "Profile updated successfully." });
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? string.Empty));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(ClaimTypes.Email, user.Email)
        };

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpireMinutes"])),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }


    private static class Crypt
    {
        public static bool Verify(string text, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(text,hash);
        }

        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
    }
}
