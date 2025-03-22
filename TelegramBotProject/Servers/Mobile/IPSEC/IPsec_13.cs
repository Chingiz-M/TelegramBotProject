using Microsoft.Extensions.Logging;
using TelegramBotProject.BaseClasses;
using TelegramBotProject.TelegramBot;

namespace TelegramBotProject.Servers.Mobile.IPSEC
{
    internal class IPsec_13 : IPSec
    {
        public IPsec_13(ILogger<IPsec_13> logger) : base(logger)
        {
            ServerIPSec = "https://89.221.225.188:8433";
            NameCertainIPSec = TgBotHostedService.IPSEC_SERVERS_LIST[12]; // IPSEC_13
        }
    }
} 

