using Microsoft.Extensions.Logging;
using TelegramBotProject.BaseClasses;
using TelegramBotProject.TelegramBot;

namespace TelegramBotProject.Services
{
    internal class Socks_1 : SOCKS
    {
        public Socks_1(ILogger<Socks_1> logger) : base(logger)
        {
            ServerSocks = "http://94.131.114.216:8433";
            NameCertainSocks = TgBotHostedService.SOCKS_SERVERS_LIST[0]; // SOCKS_1
        }     
    }
}
