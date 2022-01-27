// See https://aka.ms/new-console-template for more information

using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using LecsReminder;
using LecsReminder.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using File = System.IO.File;

var client = new TelegramBotClient("5224509753:AAHprSSvjjoQpq7SjsXZJn15PZb03tabq3g");
var adminsIds = new long[]
{
    348475752
};
//const bool isDeployed = false;
//const long groupId = isDeployed ?  -1001735383640 : -775864540;
string previousCommand = String.Empty;
await client.SetMyCommandsAsync(new[]
{
     new BotCommand {Command = "add_reminder",Description = "اضافة مذكرة والبيانات مفصولة بنقطة"},
     new BotCommand {Command = "help",Description = "مساعدة"},
     new BotCommand {Command = "clear",Description = "تصفير جميع المذكرات"},
     new BotCommand {Command = "view_reminders",Description = "عرض جميع المذكرات على شكل جدول "},
     new BotCommand {Command = "delete_reminder",Description = "حذف مذكرة"}
});
 client.StartReceiving((botClient, update, ctoken) =>
{       
    // Console.WriteLine(JsonSerializer.Serialize(update.Message.Chat.Id));
    if (update.Type != UpdateType.Message)
    {
        return;
    }
   // Console.WriteLine(JsonSerializer.Serialize(update.Message));

    var chatId = update.Message.Chat.Id;
    var text = update.Message.Text ?? string.Empty;
    if (text.Contains("/help"))
    {
        HelperCommand(botClient,chatId);
        return;
    }

    if (text.Contains("/add_reminder"))
    {
        if (!adminsIds.Contains(update.Message.From.Id))
        {
            botClient.SendTextMessageAsync(chatId,"عذرا لست ادمن , لا يمكن الاضافة");
            return;
        }
        previousCommand = "/add_reminder";
        //Console.WriteLine(previousCommand);
        botClient.SendTextMessageAsync(chatId, "اضافة مذكرة والبيانات مفصولة بنقطة : اسم المادة,موعد المادة");
        return;
    }
    if (text.Contains("/clear"))
    {
        if (!adminsIds.Contains(update.Message.From.Id))
        {
            botClient.SendTextMessageAsync(chatId,"عذرا لست ادمن ");
            return;
        }
        ClearReminders(botClient,chatId);
    }

    if (text.Contains("/view_reminders"))
    {
        string result = "ID||الموعد||اسم المادة";
        result += "\n";
        var json = File.ReadAllText("reminders.json");
        var reminders = json != string.Empty
            ? JsonSerializer.Deserialize<IEnumerable<Reminder>>(json)
                .Where(r => r.ChatId == chatId)
                .ToArray()
            : Enumerable.Empty<Reminder>().ToArray();
        //Console.WriteLine(JsonSerializer.Serialize(reminders));
        if (!reminders.Any())
        {
            botClient.SendTextMessageAsync(chatId, "لا توجد مذكرات");
            return;
        }

        foreach (var reminder in reminders)
        {
            result += $"\t{reminder.Id}||{reminder.SubjectName}||{reminder.DateTime}\n";
        }
        botClient.SendTextMessageAsync(chatId, result);
        return;
    }

    if (int.TryParse(text,out var reminderId) && previousCommand == "/remove_reminder")
    {
        var json = File.ReadAllText("reminders.json");
        if (json == String.Empty)
        {
            botClient.SendTextMessageAsync(chatId, "لا يوجد مذكرات");
            return;
        }

        var reminders = JsonSerializer.Deserialize<IEnumerable<Reminder>>(json)
            .Where(r => r.ChatId == chatId)
            .ToArray();
        if (!reminders.Any(r => r.Id == reminderId))
        {
            botClient.SendTextMessageAsync(chatId, "لا يوجد مذكرات");
            return;
        }

        reminders = reminders.Where(r => r.Id != reminderId).ToArray();
        File.WriteAllText("reminders.json",JsonSerializer.Serialize(reminders));
        botClient.SendTextMessageAsync(chatId, "تم حذف المذكرة");
        previousCommand = string.Empty;
        return;
    }
    if (text.Contains("/remove_reminder"))
    {
        if (!adminsIds.Contains(update.Message.From.Id))
        {
            botClient.SendTextMessageAsync(chatId,"عذرا لست ادمن ");
            return;
        }

        previousCommand = "/remove_reminder";
        botClient.SendTextMessageAsync(chatId,"يرجى ارسال رقم المذكرة للحذف");
    }
    if (text.Contains(','))
    {
      AddReminder(client,chatId,text.Split(','),update.Message.From);
    }
}, (botClient, exception, arg3) =>
{
    Console.BackgroundColor = ConsoleColor.Red;
    Console.WriteLine(exception.Message);
});

async void ClearReminders(ITelegramBotClient client, long chatId)
{
    await File.WriteAllTextAsync("reminders.json", "");
    await client.SendTextMessageAsync(chatId, "تم حذف جميع المذكرات");
}
async void AddReminder(ITelegramBotClient client , long chatId , string[] data ,User user )
{
    if (!adminsIds.Contains(user.Id))
    {
        await client.SendTextMessageAsync(chatId,"عذرا لست ادمن , لا يمكن الاضافة");
        return;
    }

    if (previousCommand != "/add_reminder")
    {
        await client.SendTextMessageAsync(chatId, "لا يمكن الاضافة يجب استدعاء امر الاضافة اولا");
        return;
    }
    if (!File.Exists("reminders.json"))
    {
        await using var fs = File.Create("reminders.json");
        await fs.DisposeAsync();
    }
    var subjectName = data[0];
    var dateTime = DateTime.Parse(data[1]);
    var fileText = await File.ReadAllTextAsync("reminders.json");
    
    var reminders  = fileText.Length > 0 ?  JsonSerializer.Deserialize<List<Reminder>>(fileText) :  new List<Reminder>();
    var id = new Random().Next(int.MaxValue);
    reminders.Add(new Reminder(id,subjectName,dateTime,chatId,user));
    await File.WriteAllTextAsync("reminders.json",JsonSerializer.Serialize(reminders));
    await client.SendTextMessageAsync(chatId, $"{subjectName} تمت اضافة");
    previousCommand = string.Empty;
}

async void HelperCommand(ITelegramBotClient client , long chatId)
{
    await client.SendTextMessageAsync(chatId,"قائمة الاوامر\n" +
                                             "1-اضافة مذكرة \n" +
                                             "2-حذف مذكرة \n" +
                                             "3-عرض المذكرات");
}
Console.ReadKey();