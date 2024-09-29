using Telegram.Bot;
using Telegram.Bot.Types.Payments;
using TelegramBotProject.Entities;
using TelegramBotProject.TelegramBot;

namespace TelegramBotProject.Intarfaces
{
    internal interface IMobileMethods
    {
        Task Mobile_BotStartAsync(ITelegramBotClient botClient, long chatid);
        Task Mobile_SelectServiceAsync(ITelegramBotClient botClient, long chatid, TgBotHostedService.TypeConnect connect);
        Task Mobile_SelectOpSysAsync(ITelegramBotClient botClient, long chatid, string? FirstName, string ServiceName);
        Task Mobile_BeginFreePeriodAsync(ITelegramBotClient botClient, Telegram.Bot.Types.CallbackQuery button, string typeconnect);
        Task Mobile_NewPaymentAsync(ITelegramBotClient botClient, SuccessfulPayment? payment, Telegram.Bot.Types.Update update);
        Task<IIPsec1> Mobile_CreateAndSendConfig_IpSec_IOS(ITelegramBotClient botClient, long chatID, int numserver);
        Task<IIPsec1> Mobile_CreateAndSendConfig_IpSec_Android(ITelegramBotClient botClient, long chatID, int numserver);
        Task<(ISocks1 socksServer, long id_newUser)> Mobile_CreateAndSendKey_Socks(ITelegramBotClient botClient, long chatID, string os, int numserver);
    }
}
