using Microsoft.Extensions.Logging;
using TelegramBotProject.BaseClasses;
using TelegramBotProject.TelegramBot;

namespace TelegramBotProject.Services
{
    internal class Socks_2 : SOCKS
    {
        public Socks_2(ILogger<Socks_2> logger) : base(logger)
        {
            ServerSocks = "http://45.87.153.119:8433";
            NameCertainSocks = TgBotHostedService.SOCKS_SERVERS_LIST[1]; // SOCKS_2
        }
    }
}
