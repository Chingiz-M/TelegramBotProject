using Telegram.Bot;
using Telegram.Bot.Types.Payments;
using TelegramBotProject.Entities;
using TelegramBotProject.TelegramBot;

namespace TelegramBotProject.Intarfaces
{
    internal interface IBotCommands
    {
        Task BotStartBotSelectTypeDeviceAsync(ITelegramBotClient botClient, long chatid);        
        Task BotSelectSendInvoiceAsync(ITelegramBotClient botClient, long chatid, string typepayment, string typedevice);
        Task BotSendInvoiceAsync(ITelegramBotClient botClient, long chatid, string typepayment, int months, string typedevice);
        Task BotContinuePaymentAsync(ITelegramBotClient botClient, long chatID, SuccessfulPayment? payment);
        Task BotAddReferalProgramAsync(ITelegramBotClient botClient, long chatid, long clientID);                                     
        Task BotCheckStatusUserAndSendContinuePayInvoice(ITelegramBotClient botClient, long chatID, string typepayment, int months, string typedevice);
        Task<UserDB> BotCheckUserBDAsync(long chatID, int searchType);       
        Task<int> BotSendMessageToUsersInDBAsync(ITelegramBotClient botClient, string typeService, string text, int numserver);
        Task BotCheckAndUsePromoAsync(ITelegramBotClient botClient, long chatID);
        Task BotTakeOffDBPromoAsync(ITelegramBotClient botClient, long chatID);
        Task BotMarkBlatnoiAsync(ITelegramBotClient botClient, long chatID, long adminID);
        Task BotAddDaysToUserAsync(ITelegramBotClient botClient, long chatID, int days, long adminID);
    }
}
