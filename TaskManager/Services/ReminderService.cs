using MailKit.Net.Smtp;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using TaskManagerAPI.Data;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace TaskManagerAPI.Services
{
    public class ReminderService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly IConfiguration _config;

        public ReminderService(IServiceProvider services, IConfiguration config)
        {
            _services = services;
            _config = config;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _services.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var tomorrow = DateTime.Now.Date.AddDays(1);
                    var tasks = await db.Tasks
                        .Where(t => !t.IsCompleted && t.DueDate.Date == tomorrow)
                        .ToListAsync();

                    foreach (var task in tasks)
                    {
                        await SendEmailAsync("abc@gmail.com", $"Reminder: {task.Title}",
                            $"Task '{task.Title}' is due tomorrow ({task.DueDate:d}).");
                    }
                }
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken); // check daily
            }
        }

        private async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("Task Manager", _config["Email:From"]));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;
            email.Body = new TextPart("plain") { Text = message };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_config["Email:SmtpHost"], int.Parse(_config["Email:SmtpPort"]!), false);
            await smtp.AuthenticateAsync(_config["Email:Username"], _config["Email:Password"]);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}
