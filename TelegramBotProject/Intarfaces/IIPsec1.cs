using Telegram.Bot;

namespace TelegramBotProject.Intarfaces
{
    internal interface IIPsec1
    {
        Task<string> DeleteUserAsync(long clientID);
        Task<string> RevokeUserAsync(long clientID);
        Task<int> GetTotalUserAsync();
        Task CreateUserConfigAsync(ITelegramBotClient botClient, long chatid, string funcname, string fileextension);
        string IPSecName { get; }
        string ServerIPSec { get; set; }
        string NameCertainIPSec { get; set; }
    }
}
