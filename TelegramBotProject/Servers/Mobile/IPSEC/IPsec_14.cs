using Microsoft.Extensions.Logging;
using TelegramBotProject.BaseClasses;
using TelegramBotProject.TelegramBot;

namespace TelegramBotProject.Servers.Mobile.IPSEC
{
    internal class IPsec_14 : IPSec
    {
        public IPsec_14(ILogger<IPsec_14> logger) : base(logger)
        {
            ServerIPSec = "https://89.221.225.189:8433";
            NameCertainIPSec = TgBotHostedService.IPSEC_SERVERS_LIST[13]; // IPSEC_14
        }
    }
} 

