using Microsoft.Extensions.Logging;
using TelegramBotProject.BaseClasses;
using TelegramBotProject.TelegramBot;

namespace TelegramBotProject.Servers.Mobile.IPSEC
{
    internal class IPsec_10 : IPSec
    {
        public IPsec_10(ILogger<IPsec_10> logger) : base(logger)
        {
            ServerIPSec = "https://147.45.71.74:8433";
            NameCertainIPSec = TgBotHostedService.IPSEC_SERVERS_LIST[9]; // IPSEC_10
        }
    }
} 

