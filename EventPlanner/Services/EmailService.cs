using MailKit.Net.Smtp;
using MimeKit;
using System.Threading.Tasks;
using EventPlanner.Models;
using Microsoft.Extensions.Options;

namespace EventPlanner.Services;

public class EmailService(IOptions<EmailSettings> emailSettings)
{
    private readonly EmailSettings emailSettings = emailSettings.Value;

    public async Task SendEmailAsync(string email, string subject, string message)
    {
        var emailMessage = new MimeMessage();

        emailMessage.From.Add(new MailboxAddress(emailSettings.SenderName, emailSettings.SenderEmail));
        emailMessage.To.Add(new MailboxAddress("", email));
        emailMessage.Subject = subject;
        emailMessage.Body = new TextPart("plain") { Text = message };

        using (var client = new SmtpClient())
        {
            await client.ConnectAsync(emailSettings.SmtpServer, emailSettings.Port, false);
            await client.AuthenticateAsync(emailSettings.Username, emailSettings.Password);
            await client.SendAsync(emailMessage);
            await client.DisconnectAsync(true);
        }
    }
}