using System.Text.Json;
using LecsReminder;
using Telegram.Bot;

namespace ReminderSender;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> logger;

    public Worker(ILogger<Worker> logger)
    {
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            TelegramBotClient botClient = new ("5224509753:AAHprSSvjjoQpq7SjsXZJn15PZb03tabq3g");
            var json = File.ReadAllText("reminders.json");
            var reminders = JsonSerializer.Deserialize<IEnumerable<Reminder>>(json);
            foreach (var reminder in reminders.Where(r => r.DateTime.Date == DateTime.Today))
            {
                await botClient.SendTextMessageAsync(reminder.ChatId,string.Format("{0} at {1}",reminder.SubjectName,reminder.DateTime));
                
            }
        }
    }
}