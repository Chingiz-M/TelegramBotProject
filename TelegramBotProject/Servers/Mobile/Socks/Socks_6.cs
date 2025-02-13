using Microsoft.Extensions.Logging;
using TelegramBotProject.BaseClasses;
using TelegramBotProject.TelegramBot;

namespace TelegramBotProject.Servers.Mobile.Socks
{
    internal class Socks_6 : SOCKS
    {
        public Socks_6(ILogger<Socks_6> logger) : base(logger)
        {
            ServerSocks = "http://77.239.114.51:8433";
            NameCertainSocks = TgBotHostedService.SOCKS_SERVERS_LIST[5]; // SOCKS_6
        }
    }
}
