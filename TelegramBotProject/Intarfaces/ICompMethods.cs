using Telegram.Bot;
using Telegram.Bot.Types.Payments;
using TelegramBotProject.Entities;
using TelegramBotProject.TelegramBot;

namespace TelegramBotProject.Intarfaces
{
    internal interface ICompMethods
    {
        Task BotStartCompAsync(ITelegramBotClient botClient, long chatid);

    }
}
