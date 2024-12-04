using Microsoft.Extensions.Logging;
using TelegramBotProject.BaseClasses;
using TelegramBotProject.TelegramBot;

namespace TelegramBotProject.Servers.Mobile.IPSEC
{
    internal class IPsec_7 : IPSec
    {
        public IPsec_7(ILogger<IPsec_7> logger) : base(logger)
        {
            ServerIPSec = "https://78.153.130.31:8433";
            NameCertainIPSec = TgBotHostedService.IPSEC_SERVERS_LIST[6]; // IPSEC_7
        }
    }
} 

