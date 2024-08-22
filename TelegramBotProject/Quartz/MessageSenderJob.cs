using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;
using System.Text.RegularExpressions;
using Telegram.Bot;
using TelegramBotProject.DB;
using TelegramBotProject.Intarfaces;
using TelegramBotProject.TelegramBot;
using static TelegramBotProject.StartUp;

namespace TelegramBotProject.Quartz
{
    /// <summary>
    /// В этом классе в методе Execute выбираю на какои именно серваке удалить конфиг пользователя который не оплатил подписку или же продлить 
    /// </summary>
    internal class MessageSenderJob : IJob
    {
        private readonly ILogger<MessageSenderJob> logger;
        private readonly IPSecServiceResolver ipsecResolver;
        private readonly SOCKSServiceResolver socksResolver;
        private readonly IBotCommands botcommands;

        public MessageSenderJob(ILogger<MessageSenderJob> logger, IPSecServiceResolver ipsecResolver, SOCKSServiceResolver socksResolver, IBotCommands botcommands)
        {
            this.logger = logger;
            this.ipsecResolver = ipsecResolver;
            this.socksResolver = socksResolver;
            this.botcommands = botcommands;
        }

        /// <summary>
        /// Алгоритм планировщика задач для предупреждения пользователей о необходимости оплатить подписку или удалить конфиг с определенного сервака
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                using (TgVpnbotContext db = new TgVpnbotContext())
                {
                    // Получаю всех пользователей у которых через 3 дня конец подписки и шлю им инвойс
                    var users_payment_3 = await db.Users.Where(u => u.DateNextPayment.Date == DateTime.Now.AddDays(2).Date && u.Status == "active" && u.Blatnoi == false).ToListAsync().ConfigureAwait(false);
                    var users_payment_2 = await db.Users.Where(u => u.DateNextPayment.Date == DateTime.Now.AddDays(1).Date && u.Status == "active" && u.Blatnoi == false).ToListAsync().ConfigureAwait(false);
                    var users_payment_1 = await db.Users.Where(u => u.DateNextPayment.Date == DateTime.Now.Date && u.Status == "active" && u.Blatnoi == false).ToListAsync().ConfigureAwait(false);
                    var users_payment_0 = await db.Users.Where(u => u.DateNextPayment.Date == DateTime.Now.AddDays(-1).Date && u.Status == "active" && u.Blatnoi == false).ToListAsync().ConfigureAwait(false);

                    int count_continue = users_payment_3.Count + users_payment_2.Count + users_payment_1.Count;

                    var date_3 = DateTime.Now.AddDays(3).ToString("dd-MM-yyyy");
                    var date_2 = DateTime.Now.AddDays(2).ToString("dd-MM-yyyy");
                    var date_1 = DateTime.Now.AddDays(1).ToString("dd-MM-yyyy");

                    foreach (var user in users_payment_3)
                    {
                        try
                        {
                            await TgBotHostedService.bot.SendTextMessageAsync(user.ChatID, $"Привет, {user?.FirstName}! 👋\n\n" +
                            $"Напоминаем, что через 3 дня ({date_3}) заканчивается твоя подписка 🥺\n\n" +
                            $"Если тебе понравился наш сервис и ты хочешь продлить подписку, то выбери вариант продления ниже ⬇️");

                            await botcommands.BotSelectSendInvoiceAsync(TgBotHostedService.bot, user.ChatID, NamesInlineButtons.ContinuePayment);
                        }
                        catch (Exception ex) { logger.LogInformation("Ошибка в планировщике payment 3, exeption: {ex}", ex); }                                              
                    }

                    foreach (var user in users_payment_2)
                    {
                        try
                        {
                            await TgBotHostedService.bot.SendTextMessageAsync(user.ChatID, $"Привет, {user?.FirstName}! 👋\n\n" +
                            $"Напоминаем, что через 2 дня ({date_2}) заканчивается твоя подписка 🥺\n\n" +
                            $"Если тебе понравился наш сервис и ты хочешь продлить подписку, то выбери 👀 вариант продления ниже ⬇️");

                            await botcommands.BotSelectSendInvoiceAsync(TgBotHostedService.bot, user.ChatID, NamesInlineButtons.ContinuePayment);
                        }
                        catch (Exception ex) { logger.LogInformation("Ошибка в планировщике payment 2, exeption: {ex}", ex); }                       
                    }

                    foreach (var user in users_payment_1)
                    {
                        try
                        {
                            await TgBotHostedService.bot.SendTextMessageAsync(user.ChatID, $"Привет, {user?.FirstName}! 👋\n\n" +
                            $"Уже завтра 😱 ({date_1}) заканчивается твоя подписка 😢\n\n" +
                            $"Если тебе понравился наш сервис и ты хочешь продлить подписку, то выбери 👀 вариант продления ниже ⬇️");

                            await botcommands.BotSelectSendInvoiceAsync(TgBotHostedService.bot, user.ChatID, NamesInlineButtons.ContinuePayment);
                        }
                        catch (Exception ex) { logger.LogInformation("Ошибка в планировщике payment 1, exeption: {ex}", ex); }                      
                    }

                    foreach (var user in users_payment_0) // не заплатил и подписка закончилась
                    {
                        try
                        {
                            var match_i = Regex.Match(user.NameService.ToLower(), @"ipsec_\d+");
                            var match_s = Regex.Match(user.NameService.ToLower(), @"socks_\d+");
                            if (match_i.Success)
                            {
                                var ipsecServer = ipsecResolver(user.NameService);
                                var resRevoke = await ipsecServer.RevokeUserAsync(user.ChatID);
                                var resDelete = await ipsecServer.DeleteUserAsync(user.ChatID);
                            }
                            if (match_s.Success)
                            {
                                var socksServer = socksResolver(user.NameService);
                                var res = await socksServer.DeleteUserAsync(user.ServiceKey);
                            }

                            user.Status = "nonactive";
                            user.DateDisconnect = DateTime.Now;

                            db.Users.Update(user);
                            await db.SaveChangesAsync();

                            await TgBotHostedService.bot.SendTextMessageAsync(user.ChatID, $"Привет, {user?.FirstName}! 👋\n\n" +
                                $"Твоя подписка на наш сервис закончилась 😭\n\n" +
                                $"Ты всегда можешь возобновить свою подписку и выбрать вариант подключения нажав /start");

                            logger.LogInformation("Конфиг пользователя удален, chatid: {chatid}", user.ChatID);
                        }
                        catch (Exception ex) { logger.LogInformation("Ошибка в планировщике payment 0, exeption: {ex}", ex); }                      
                    }
                    await TgBotHostedService.bot.SendTextMessageAsync(1278048494, $"Всего должны продлить {count_continue} человек");
                    await TgBotHostedService.bot.SendTextMessageAsync(1278048494, "Планировщик отработал без ошибок");

                }
            }
            catch (Exception ex)
            {
                logger.LogInformation("Ошибка в планировщике, exeption: {ex}", ex);
                await TgBotHostedService.bot.SendTextMessageAsync(1278048494, "Ошибка в Планировщике: " + ex.Message);
            }

        }
    }
}
