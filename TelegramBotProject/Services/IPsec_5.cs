using Microsoft.Extensions.Logging;
using TelegramBotProject.BaseClasses;
using TelegramBotProject.TelegramBot;

namespace TelegramBotProject.Services
{
    internal class IPsec_5 : IPSec
    {
        public IPsec_5(ILogger<IPsec_5> logger) : base(logger)
        {
            ServerIPSec = "https://45.142.214.120:8433";
            NameCertainIPSec = TgBotHostedService.IPSEC_SERVERS_LIST[4]; // IPSEC_5
        }
    }
}
