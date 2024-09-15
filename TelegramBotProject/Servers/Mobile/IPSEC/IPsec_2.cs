using Microsoft.Extensions.Logging;
using TelegramBotProject.BaseClasses;
using TelegramBotProject.TelegramBot;

namespace TelegramBotProject.Servers.Mobile.IPSEC
{
    internal class IPsec_2 : IPSec
    {
        public IPsec_2(ILogger<IPsec_2> logger) : base(logger)
        {
            ServerIPSec = "https://77.91.74.210:8433";
            NameCertainIPSec = TgBotHostedService.IPSEC_SERVERS_LIST[1]; // IPSEC_2
        }
    }
}
