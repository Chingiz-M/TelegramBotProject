using Microsoft.Extensions.Logging;
using TelegramBotProject.BaseClasses;
using TelegramBotProject.TelegramBot;

namespace TelegramBotProject.Servers.Mobile.IPSEC
{
    internal class IPsec_8 : IPSec
    {
        public IPsec_8(ILogger<IPsec_8> logger) : base(logger)
        {
            ServerIPSec = "https://138.124.90.54:8433";
            NameCertainIPSec = TgBotHostedService.IPSEC_SERVERS_LIST[7]; // IPSEC_8
        }
    }
} 

