using Microsoft.Extensions.Logging;
using TelegramBotProject.BaseClasses;
using TelegramBotProject.TelegramBot;

namespace TelegramBotProject.Servers.Comp
{
    internal class Comp_IPsec_3 : IPSec
    {
        public Comp_IPsec_3(ILogger<Comp_IPsec_3> logger) : base(logger)
        {
            ServerIPSec = "https://91.225.218.126:8433";
            NameCertainIPSec = TgBotHostedService.Comp_IPSEC_SERVERS_LIST[2]; // Comp_IPSEC_3
        }
    }
}
