using Microsoft.Extensions.Logging;
using TelegramBotProject.Intarfaces;
using static TelegramBotProject.StartUp;

namespace TelegramBotProject.Services
{
    internal class MobileMethods : IMobileMethods
    {
        private ILogger logger;
        private readonly IPSecServiceResolver ipsecResolver;
        private readonly SOCKSServiceResolver socksResolver;

        public MobileMethods(ILogger<MobileMethods> logger, IPSecServiceResolver ipsecResolver, SOCKSServiceResolver socksResolver)
        {
            this.logger = logger;
            this.ipsecResolver = ipsecResolver;
            this.socksResolver = socksResolver;
        }


    }
}
