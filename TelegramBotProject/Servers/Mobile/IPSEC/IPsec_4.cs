using Microsoft.Extensions.Logging;
using TelegramBotProject.BaseClasses;
using TelegramBotProject.TelegramBot;

namespace TelegramBotProject.Servers.Mobile.IPSEC
{
    internal class IPsec_4 : IPSec
    {
        public IPsec_4(ILogger<IPsec_4> logger) : base(logger)
        {
            ServerIPSec = "https://45.142.214.128:8433";
            NameCertainIPSec = TgBotHostedService.IPSEC_SERVERS_LIST[3]; // IPSEC_4
        }
    }
}
