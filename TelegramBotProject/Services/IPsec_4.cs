using Microsoft.Extensions.Logging;
using TelegramBotProject.BaseClasses;
using TelegramBotProject.TelegramBot;

namespace TelegramBotProject.Services
{
    internal class IPsec_4 : IPSec
    {
        public IPsec_4(ILogger<IPsec_4> logger) : base(logger)
        {
            ServerIPSec = "https://194.54.156.11:8433";
            NameCertainIPSec = TgBotHostedService.IPSEC_SERVERS_LIST[3]; // IPSEC_4
        }
    }
}
