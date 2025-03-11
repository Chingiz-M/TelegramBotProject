using Microsoft.Extensions.Logging;
using TelegramBotProject.BaseClasses;
using TelegramBotProject.TelegramBot;

namespace TelegramBotProject.Servers.Mobile.Socks
{
    internal class Socks_3 : SOCKS
    {
        public Socks_3(ILogger<Socks_3> logger) : base(logger)
        {
            ServerSocks = "http://45.82.254.131:8433";
            NameCertainSocks = TgBotHostedService.SOCKS_SERVERS_LIST[2]; // SOCKS_3
        }
    }
}
