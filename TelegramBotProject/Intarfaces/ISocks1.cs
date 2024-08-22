using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace TelegramBotProject.Intarfaces
{
    internal interface ISocks1
    {
        Task<long> CreateUserAsync(ITelegramBotClient botClient, long chatid);
        Task<string> DeleteUserAsync(long clientkey);
        Task<int> GetFileUsersAsync(string filename, bool createfile);
        Task<string> SetLimitTrafficAsync(long volume_traffic_gb);
        Task<string> DeleteLimitTrafficAsync();
        string SocksName { get; }
        string ServerSocks { get; set; }
        string NameCertainSocks { get; set; }
    }
}
