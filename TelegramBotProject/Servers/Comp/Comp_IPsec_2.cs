using Microsoft.Extensions.Logging;
using TelegramBotProject.BaseClasses;
using TelegramBotProject.TelegramBot;

namespace TelegramBotProject.Servers.Comp
{
    internal class Comp_IPsec_2 : IPSec
    {
        public Comp_IPsec_2(ILogger<Comp_IPsec_2> logger) : base(logger)
        {
            ServerIPSec = "https://94.131.99.184:8433";
            NameCertainIPSec = TgBotHostedService.Comp_IPSEC_SERVERS_LIST[1]; // Comp_IPSEC_2
        }
    }
}
