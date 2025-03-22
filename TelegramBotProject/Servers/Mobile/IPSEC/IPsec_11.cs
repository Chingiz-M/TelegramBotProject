using Microsoft.Extensions.Logging;
using TelegramBotProject.BaseClasses;
using TelegramBotProject.TelegramBot;

namespace TelegramBotProject.Servers.Mobile.IPSEC
{
    internal class IPsec_11 : IPSec
    {
        public IPsec_11(ILogger<IPsec_11> logger) : base(logger)
        {
            ServerIPSec = "https://89.221.225.182:8433";
            NameCertainIPSec = TgBotHostedService.IPSEC_SERVERS_LIST[10]; // IPSEC_11
        }
    }
} 

