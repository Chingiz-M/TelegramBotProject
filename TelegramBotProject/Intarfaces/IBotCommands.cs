using Telegram.Bot;
using Telegram.Bot.Types.Payments;
using TelegramBotProject.Entities;
using TelegramBotProject.TelegramBot;

namespace TelegramBotProject.Intarfaces
{
    internal interface IBotCommands
    {
        Task BotStartAsync(ITelegramBotClient botClient, long chatid);
        Task BotSelectServiceAsync(ITelegramBotClient botClient, long chatid, TgBotHostedService.TypeConnect connect);
        Task BotStartBotSelectTypeDeviceAsync(ITelegramBotClient botClient, long chatid);
        Task BotSelectOpSysAsync(ITelegramBotClient botClient, long chatid, string? FirstName, string ServiceName);
        Task BotSelectSendInvoiceAsync(ITelegramBotClient botClient, long chatid, string typepayment);
        Task BotSendInvoiceAsync(ITelegramBotClient botClient, long chatid, string typepayment, int months);
        Task BotNewPaymentAsync(ITelegramBotClient botClient, SuccessfulPayment? payment, Telegram.Bot.Types.Update update);
        Task BotContinuePaymentAsync(ITelegramBotClient botClient, long chatID, SuccessfulPayment? payment);
        Task BotAddReferalProgramAsync(ITelegramBotClient botClient, long chatid, long clientID);                                     
        Task BotCheckStatusUserAndSendContinuePayInvoice(ITelegramBotClient botClient, long chatID, string typepayment, int months);
        Task<UserDB> BotCheckUserBDAsync(long chatID, int searchType);
        Task<IIPsec1> CreateAndSendConfig_IpSec_IOS(ITelegramBotClient botClient, long chatID, int numserver);
        Task<IIPsec1> CreateAndSendConfig_IpSec_Android(ITelegramBotClient botClient, long chatID, int numserver);
        Task<(ISocks1 socksServer, long id_newUser)> CreateAndSendKey_Socks(ITelegramBotClient botClient, long chatID, string os, int numserver);
        Task BotBeginFreePeriodAsync(ITelegramBotClient botClient, Telegram.Bot.Types.CallbackQuery button, string typeconnect);
        Task<int> BotSendMessageToUsersInDBAsync(ITelegramBotClient botClient, string typeService, string text);
        Task BotCheckAndUsePromoAsync(ITelegramBotClient botClient, long chatID);
        Task BotTakeOffDBPromoAsync(ITelegramBotClient botClient, long chatID);
        Task BotMarkBlatnoiAsync(ITelegramBotClient botClient, long chatID, long adminID);
    }
}
