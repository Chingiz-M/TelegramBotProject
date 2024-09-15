using Telegram.Bot;
using Telegram.Bot.Types.Payments;
using TelegramBotProject.Entities;
using TelegramBotProject.TelegramBot;

namespace TelegramBotProject.Intarfaces
{
    internal interface ICompMethods
    {
        Task Comp_BotStartAsync(ITelegramBotClient botClient, long chatid);

        Task Comp_SelectOpSysAsync(ITelegramBotClient botClient, long chatid, string ServiceName);
        Task Comp_BotBeginFreePeriodAsync(ITelegramBotClient botClient, Telegram.Bot.Types.CallbackQuery button, string typeconnect);


        Task<IIPsec1> Comp_CreateAndSendConfig_IpSec_MacOS(ITelegramBotClient botClient, long chatID, int numserver);
        Task<IIPsec1> Comp_CreateAndSendConfig_IpSec_Windows(ITelegramBotClient botClient, long chatID, int numserver);

    }
}
