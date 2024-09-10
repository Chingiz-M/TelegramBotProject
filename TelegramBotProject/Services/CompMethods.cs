using Microsoft.Extensions.Logging;
using Telegram.Bot;
using TelegramBotProject.Intarfaces;
using static TelegramBotProject.StartUp;

namespace TelegramBotProject.Services
{
    internal class CompMethods : ICompMethods
    {
        private ILogger logger;
        private readonly IPSecServiceResolver ipsecResolver;
        private readonly SOCKSServiceResolver socksResolver;

        public CompMethods(ILogger<CompMethods> logger, IPSecServiceResolver ipsecResolver, SOCKSServiceResolver socksResolver)
        {
            this.logger = logger;
            this.ipsecResolver = ipsecResolver;
            this.socksResolver = socksResolver;
        }

        public Task BotStartCompAsync(ITelegramBotClient botClient, long chatid)
        {
            throw new NotImplementedException();
        }
    }
}
