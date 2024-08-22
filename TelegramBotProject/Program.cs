using Microsoft.Extensions.Hosting;
//using Telegram.Bots.Types;

namespace TelegramBotProject
{
    class Program
    {
        static public IHost HostApp { get; set; }

        static async Task Main(string[] args)
        {
            HostApp = StartUp.CreateApplicationHost();
            await HostApp.RunAsync();
        }
        
    }

}