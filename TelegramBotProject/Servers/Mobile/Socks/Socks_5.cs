using Microsoft.Extensions.Logging;
using TelegramBotProject.BaseClasses;
using TelegramBotProject.TelegramBot;

namespace TelegramBotProject.Servers.Mobile.Socks
{
    internal class Socks_5 : SOCKS
    {
        public Socks_5(ILogger<Socks_5> logger) : base(logger)
        {
            ServerSocks = "http://45.82.254.142:8433";
            NameCertainSocks = TgBotHostedService.SOCKS_SERVERS_LIST[4]; // SOCKS_5
        }
    }
}
