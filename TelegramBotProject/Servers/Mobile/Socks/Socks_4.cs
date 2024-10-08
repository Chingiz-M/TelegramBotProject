using Microsoft.Extensions.Logging;
using TelegramBotProject.BaseClasses;
using TelegramBotProject.TelegramBot;

namespace TelegramBotProject.Servers.Mobile.Socks
{
    internal class Socks_4 : SOCKS
    {
        public Socks_4(ILogger<Socks_4> logger) : base(logger)
        {
            ServerSocks = "http://5.182.36.26:8433";
            NameCertainSocks = TgBotHostedService.SOCKS_SERVERS_LIST[3]; // SOCKS_4
        }
    }
}
