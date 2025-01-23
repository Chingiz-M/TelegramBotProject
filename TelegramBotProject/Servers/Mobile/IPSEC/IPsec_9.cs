using Microsoft.Extensions.Logging;
using TelegramBotProject.BaseClasses;
using TelegramBotProject.TelegramBot;

namespace TelegramBotProject.Servers.Mobile.IPSEC
{
    internal class IPsec_9 : IPSec
    {
        public IPsec_9(ILogger<IPsec_9> logger) : base(logger)
        {
            ServerIPSec = "https://62.60.238.14:8433";
            NameCertainIPSec = TgBotHostedService.IPSEC_SERVERS_LIST[8]; // IPSEC_9
        }
    }
} 

