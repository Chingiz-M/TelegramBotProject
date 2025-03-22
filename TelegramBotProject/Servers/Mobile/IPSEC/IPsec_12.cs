using Microsoft.Extensions.Logging;
using TelegramBotProject.BaseClasses;
using TelegramBotProject.TelegramBot;

namespace TelegramBotProject.Servers.Mobile.IPSEC
{
    internal class IPsec_12 : IPSec
    {
        public IPsec_12(ILogger<IPsec_12> logger) : base(logger)
        {
            ServerIPSec = "https://89.221.225.187:8433";
            NameCertainIPSec = TgBotHostedService.IPSEC_SERVERS_LIST[11]; // IPSEC_12
        }
    }
} 

