using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using Serilog;
using TelegramBotProject.BaseClasses;
using TelegramBotProject.Intarfaces;
using TelegramBotProject.Quartz;
using TelegramBotProject.Servers.Comp;
using TelegramBotProject.Servers.Mobile.IPSEC;
using TelegramBotProject.Servers.Mobile.Socks;
using TelegramBotProject.Services;
using TelegramBotProject.TelegramBot;

namespace TelegramBotProject
{
    public class StartUp
    {
        internal delegate IIPsec1 Comp_IPSecServiceResolver(string key);
        internal delegate IIPsec1 IPSecServiceResolver(string key);
        internal delegate ISocks1 SOCKSServiceResolver(string key);

        public static string GetTokenfromConfig(string type)
        {
            var builder = new ConfigurationBuilder()
                        .AddJsonFile($"appsettings.json", true, true);
            var config = builder.Build();
            string Token = config[$"Config:{type}"].ToString();
            return Token;
        }

        public static IConfiguration GetIConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"appsettings.json", true, true);
            var config = builder.Build();
            return config;
        }

        public static IHost CreateApplicationHost()
        {
            Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(StartUp.GetIConfiguration())
                    .CreateLogger();

            var host = Host.CreateDefaultBuilder() // Initialising the Host 
              .ConfigureServices((context, services) => {

                  services.AddHostedService<TgBotHostedService>();
                  services.AddTransient<IBotCommands, BotComands>();
                  services.AddTransient<IMobileMethods, MobileMethods>();
                  services.AddTransient<ICompMethods, CompMethods>();

                  // SOCKS SETTINGS
                  services.AddTransient<SOCKS>();
                  services.AddTransient<Socks_1>();
                  services.AddTransient<Socks_2>();
                  services.AddTransient<Socks_3>();
                  services.AddTransient<Socks_4>();

                  services.AddTransient<SOCKSServiceResolver>(serviceProvider => key =>
                  {
                      switch (key)
                      {
                          case "SOCKS_0":
                              return serviceProvider.GetService<SOCKS>();
                          case "SOCKS_1":
                              return serviceProvider.GetService<Socks_1>();
                          case "SOCKS_2":
                              return serviceProvider.GetService<Socks_2>();
                          case "SOCKS_3":
                              return serviceProvider.GetService<Socks_3>();
                          case "SOCKS_4":
                              return serviceProvider.GetService<Socks_4>();
                          default:
                              throw new KeyNotFoundException(); // or maybe return null, up to you
                      }
                  });

                  // IPSEC SETTINGS
                  services.AddTransient<IPSec>();
                  services.AddTransient<IPsec_1>();
                  services.AddTransient<IPsec_2>();
                  services.AddTransient<IPsec_3>();
                  services.AddTransient<IPsec_4>();
                  services.AddTransient<IPsec_5>();

                  services.AddTransient<IPSecServiceResolver>(serviceProvider => key =>
                  {
                      switch (key)
                      {
                          case "IPSEC_0":
                              return serviceProvider.GetService<IPSec>();
                          case "IPSEC_1":
                              return serviceProvider.GetService<IPsec_1>();
                          case "IPSEC_2":
                              return serviceProvider.GetService<IPsec_2>();
                          case "IPSEC_3":
                              return serviceProvider.GetService<IPsec_3>();
                          case "IPSEC_4":
                              return serviceProvider.GetService<IPsec_4>();
                          case "IPSEC_5":
                              return serviceProvider.GetService<IPsec_5>();
                          default:
                              throw new KeyNotFoundException(); // or maybe return null, up to you
                      }
                  });

                  // COMP IPSEC SETTINGS
                  services.AddTransient<Comp_IPsec_1>();
                  services.AddTransient<Comp_IPsec_2>();

                  services.AddTransient<Comp_IPSecServiceResolver>(serviceProvider => key =>
                  {
                      switch (key)
                      {
                          case "Comp_IPSEC_1":
                              return serviceProvider.GetService<Comp_IPsec_1>();
                          case "Comp_IPSEC_2":
                              return serviceProvider.GetService<Comp_IPsec_2>();
                          default:
                              throw new KeyNotFoundException(); // or maybe return null, up to you
                      }
                  });

                  // QUARTZ SETTINGS

                  services.AddQuartz(q =>
                  {
                      q.UseMicrosoftDependencyInjectionJobFactory();

                      var jobKey = new JobKey("MessageSenderJob");

                      // Register the job with the DI container
                      q.AddJob<MessageSenderJob>(opts => opts.WithIdentity(jobKey));

                      // Create a trigger for the job
                      q.AddTrigger(opts => opts
                        .ForJob(jobKey)
                        .WithIdentity("MessageSenderJob-trigger") // give the trigger a unique name
                        .StartNow()                            // запуск сразу после начала выполнения
                                                               //.WithSimpleSchedule(x => x            // настраиваем выполнение действия
                                                               //    .WithIntervalInSeconds(200)          // через 200 sec
                                                               //    .RepeatForever()));
                                                               //.WithCronSchedule("0 */2 * ? * *")); // run every 2 min
                                                                    .WithCronSchedule("0 0 12 ? * * *")); // run every day at 12 o'clock at London
                  });

                  services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

              })
              .UseSerilog() // Add Serilog
              .Build(); // Build the Host

            return host;
        }
    }

}
