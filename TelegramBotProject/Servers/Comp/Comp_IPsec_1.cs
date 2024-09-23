using Microsoft.Extensions.Logging;
using TelegramBotProject.BaseClasses;
using TelegramBotProject.TelegramBot;

namespace TelegramBotProject.Servers.Comp
{
    internal class Comp_IPsec_1 : IPSec
    {
        public Comp_IPsec_1(ILogger<Comp_IPsec_1> logger) : base(logger)
        {
            ServerIPSec = "https://194.31.109.190:8433";
            NameCertainIPSec = TgBotHostedService.Comp_IPSEC_SERVERS_LIST[0]; // Comp_IPSEC_1
        }
    }
}
