using Microsoft.Extensions.Logging;
using TelegramBotProject.BaseClasses;
using TelegramBotProject.TelegramBot;

namespace TelegramBotProject.Servers.Mobile.IPSEC
{
    internal class IPsec_1 : IPSec
    {
        public IPsec_1(ILogger<IPsec_1> logger) : base(logger)
        {
            ServerIPSec = "https://176.124.200.227:8433";
            NameCertainIPSec = TgBotHostedService.IPSEC_SERVERS_LIST[0]; // IPSEC_1
        }
    }
}
