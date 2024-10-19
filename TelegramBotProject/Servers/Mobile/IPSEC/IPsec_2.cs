using Microsoft.Extensions.Logging;
using TelegramBotProject.BaseClasses;
using TelegramBotProject.TelegramBot;

namespace TelegramBotProject.Servers.Mobile.IPSEC
{
    internal class IPsec_2 : IPSec
    {
        public IPsec_2(ILogger<IPsec_2> logger) : base(logger)
        {
            ServerIPSec = "https://86.104.75.177:8433";
            NameCertainIPSec = TgBotHostedService.IPSEC_SERVERS_LIST[1]; // IPSEC_2
        }
    }
}
