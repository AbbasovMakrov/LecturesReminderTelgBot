using Telegram.Bot.Types;

namespace LecsReminder;

public record Reminder(long Id , string SubjectName , DateTime DateTime,long ChatId,User AddedByUser);