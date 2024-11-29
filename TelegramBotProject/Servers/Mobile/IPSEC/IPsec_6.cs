using Microsoft.Extensions.Logging;
using TelegramBotProject.BaseClasses;
using TelegramBotProject.TelegramBot;

namespace TelegramBotProject.Servers.Mobile.IPSEC
{
    internal class IPsec_6 : IPSec
    {
        public IPsec_6(ILogger<IPsec_6> logger) : base(logger)
        {
            ServerIPSec = "https://176.124.204.218:8433";
            NameCertainIPSec = TgBotHostedService.IPSEC_SERVERS_LIST[5]; // IPSEC_6
        }
    }
} 

