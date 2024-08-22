using Microsoft.Extensions.Logging;
using TelegramBotProject.BaseClasses;
using TelegramBotProject.TelegramBot;

namespace TelegramBotProject.Services
{
    internal class IPsec_3 : IPSec
    {
        public IPsec_3(ILogger<IPsec_3> logger) : base(logger)
        {
            ServerIPSec = "https://89.221.225.60:8433";
            NameCertainIPSec = TgBotHostedService.IPSEC_SERVERS_LIST[2]; // IPSEC_3
        }
    }
}
