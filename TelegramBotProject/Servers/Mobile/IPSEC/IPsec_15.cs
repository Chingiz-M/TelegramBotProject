using Microsoft.Extensions.Logging;
using TelegramBotProject.BaseClasses;
using TelegramBotProject.TelegramBot;

namespace TelegramBotProject.Servers.Mobile.IPSEC
{
    internal class IPsec_15 : IPSec
    {
        public IPsec_15(ILogger<IPsec_15> logger) : base(logger)
        {
            ServerIPSec = "https://89.221.225.247:8433";
            NameCertainIPSec = TgBotHostedService.IPSEC_SERVERS_LIST[14]; // IPSEC_15
        }
    }
} 

