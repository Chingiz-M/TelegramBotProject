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
        private readonly Comp_IPSecServiceResolver comp_ipsecResolver;
        private readonly IPSecServiceResolver ipsecResolver;
        private readonly SOCKSServiceResolver socksResolver;
        private readonly IBotCommands botcommands;

        public MessageSenderJob(ILogger<MessageSenderJob> logger, IPSecServiceResolver ipsecResolver, SOCKSServiceResolver socksResolver,
            IBotCommands botcommands, Comp_IPSecServiceResolver comp_ipsecResolver)
        {
            this.logger = logger;
            this.comp_ipsecResolver = comp_ipsecResolver;
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
                    var users_payment_3 = await db.Users.Where(u => u.DateNextPayment.Date == DateTime.UtcNow.AddDays(2).Date && u.Status == "active" && u.Blatnoi == false).ToListAsync().ConfigureAwait(false);
                    var users_payment_2 = await db.Users.Where(u => u.DateNextPayment.Date == DateTime.UtcNow.AddDays(1).Date && u.Status == "active" && u.Blatnoi == false).ToListAsync().ConfigureAwait(false);
                    var users_payment_1 = await db.Users.Where(u => u.DateNextPayment.Date == DateTime.UtcNow.Date && u.Status == "active" && u.Blatnoi == false).ToListAsync().ConfigureAwait(false);
                    var users_payment_0 = await db.Users.Where(u => u.DateNextPayment.Date == DateTime.UtcNow.AddDays(-1).Date && u.Status == "active" && u.Blatnoi == false).ToListAsync().ConfigureAwait(false);

                    int count_continue = users_payment_3.Count + users_payment_2.Count + users_payment_1.Count;
                    int count_deleted_users = 0;

                    var date_3 = DateTime.UtcNow.AddDays(3).ToString("dd-MM-yyyy");
                    var date_2 = DateTime.UtcNow.AddDays(2).ToString("dd-MM-yyyy");
                    var date_1 = DateTime.UtcNow.AddDays(1).ToString("dd-MM-yyyy");

                    foreach (var user in users_payment_3) // здесь все по датам включая chatid для компа и для телефонов
                    {
                        try
                        {                                                       
                            if (user.TypeOfDevice == NamesInlineButtons.StartComp) // если комп то нужно не обьебаться с chatid поделить на число
                            {
                                var real_chatid = user.ChatID / TgBotHostedService.USERS_COMP;
                                await TgBotHostedService.bot.SendTextMessageAsync(real_chatid, $"Привет, {user?.FirstName}! 👋\n\n" +
                                    $"Напоминаем, что через 3 дня ({date_3}) заканчивается твоя подписка на ПК 💻\n\n" +
                                    $"Если тебе понравился наш сервис и ты хочешь продлить подписку, то выбери вариант продления ниже ⬇️");

                                await botcommands.BotSelectSendInvoiceAsync(TgBotHostedService.bot, real_chatid, NamesInlineButtons.ContinuePayment_Comp, NamesInlineButtons.StartComp);
                            }
                            else // если мобилка то тут нич не нужно править
                            {
                                await TgBotHostedService.bot.SendTextMessageAsync(user.ChatID, $"Привет, {user?.FirstName}! 👋\n\n" +
                                    $"Напоминаем, что через 3 дня ({date_3}) заканчивается твоя подписка на Мобильное устройство 📱\n\n" +
                                    $"Если тебе понравился наш сервис и ты хочешь продлить подписку, то выбери вариант продления ниже ⬇️");

                                await botcommands.BotSelectSendInvoiceAsync(TgBotHostedService.bot, user.ChatID, NamesInlineButtons.ContinuePayment_Mobile, NamesInlineButtons.StartMobile);
                            }
                        }
                        catch (Exception ex) { logger.LogInformation("Ошибка в планировщике payment 3, exeption: {ex}", ex); }                                              
                    }

                    foreach (var user in users_payment_2)
                    {
                        try
                        {
                            if (user.TypeOfDevice == NamesInlineButtons.StartComp) // если комп то нужно не обьебаться с chatid поделить на число
                            {
                                var real_chatid = user.ChatID / TgBotHostedService.USERS_COMP;
                                await TgBotHostedService.bot.SendTextMessageAsync(real_chatid, $"Привет, {user?.FirstName}! 👋\n\n" +
                                    $"Напоминаем, что через 2 дня ({date_2}) заканчивается твоя подписка на ПК 💻\n\n" +
                                    $"Если тебе понравился наш сервис и ты хочешь продлить подписку, то выбери вариант продления ниже ⬇️");

                                await botcommands.BotSelectSendInvoiceAsync(TgBotHostedService.bot, real_chatid, NamesInlineButtons.ContinuePayment_Comp, NamesInlineButtons.StartComp);
                            }
                            else // если мобилка то тут нич не нужно править
                            {
                                await TgBotHostedService.bot.SendTextMessageAsync(user.ChatID, $"Привет, {user?.FirstName}! 👋\n\n" +
                                    $"Напоминаем, что через 2 дня ({date_2}) заканчивается твоя подписка на Мобильное устройство 📱\n\n" +
                                    $"Если тебе понравился наш сервис и ты хочешь продлить подписку, то выбери вариант продления ниже ⬇️");

                                await botcommands.BotSelectSendInvoiceAsync(TgBotHostedService.bot, user.ChatID, NamesInlineButtons.ContinuePayment_Mobile, NamesInlineButtons.StartMobile);
                            }
                        }
                        catch (Exception ex) { logger.LogInformation("Ошибка в планировщике payment 2, exeption: {ex}", ex); }                       
                    }

                    foreach (var user in users_payment_1)
                    {
                        try
                        {
                            if (user.TypeOfDevice == NamesInlineButtons.StartComp) // если комп то нужно не обьебаться с chatid поделить на число
                            {
                                var real_chatid = user.ChatID / TgBotHostedService.USERS_COMP;
                                await TgBotHostedService.bot.SendTextMessageAsync(real_chatid, $"Привет, {user?.FirstName}! 👋\n\n" +
                                    $"Уже завтра 😱 ({date_1}) заканчивается твоя подписка на ПК 💻\n\n" +
                                    $"Если тебе понравился наш сервис и ты хочешь продлить подписку, то выбери вариант продления ниже ⬇️");

                                await botcommands.BotSelectSendInvoiceAsync(TgBotHostedService.bot, real_chatid, NamesInlineButtons.ContinuePayment_Comp, NamesInlineButtons.StartComp);
                            }
                            else // если мобилка то тут нич не нужно править
                            {
                                await TgBotHostedService.bot.SendTextMessageAsync(user.ChatID, $"Привет, {user?.FirstName}! 👋\n\n" +
                                    $"Уже завтра 😱 ({date_1}) заканчивается твоя подписка на Мобильное устройство 📱\n\n" +
                                    $"Если тебе понравился наш сервис и ты хочешь продлить подписку, то выбери вариант продления ниже ⬇️");

                                await botcommands.BotSelectSendInvoiceAsync(TgBotHostedService.bot, user.ChatID, NamesInlineButtons.ContinuePayment_Mobile, NamesInlineButtons.StartMobile);
                            }
                        }
                        catch (Exception ex) { logger.LogInformation("Ошибка в планировщике payment 1, exeption: {ex}", ex); }                      
                    }

                    foreach (var user in users_payment_0) // не заплатил и подписка закончилась
                    {
                        try
                        {
                            long real_chatid = user.ChatID;

                            var match_i = Regex.Match(user.NameService.ToLower(), @".*ipsec_\d+");
                            var match_s = Regex.Match(user.NameService.ToLower(), @"socks_\d+");
                            if (match_i.Success)
                            {
                                IIPsec1? ipsecServer = null;

                                if (user.TypeOfDevice == NamesInlineButtons.StartComp) // если комп то выбираю его резолвер
                                    ipsecServer = comp_ipsecResolver(user.NameService); 

                                else if (user.TypeOfDevice == NamesInlineButtons.StartMobile) // если мобила то выбираю его резолвер
                                    ipsecServer = ipsecResolver(user.NameService);

                                var resRevoke = await ipsecServer.RevokeUserAsync(user.ChatID);
                                var resDelete = await ipsecServer.DeleteUserAsync(user.ChatID);
                            }
                            if (match_s.Success)
                            {
                                var socksServer = socksResolver(user.NameService);
                                var res = await socksServer.DeleteUserAsync(user.ServiceKey);
                            }

                            DateTime? datetemp = DateTime.Now;
                            user.Status = "nonactive";
                            user.DateDisconnect = DateTime.UtcNow;

                            //db.Users.Update(user);
                            await db.SaveChangesAsync();

                            if (user.TypeOfDevice == NamesInlineButtons.StartComp) // если комп то нужно не обьебаться с chatid поделить на число
                                real_chatid = user.ChatID / TgBotHostedService.USERS_COMP;



                            await TgBotHostedService.bot.SendTextMessageAsync(real_chatid, $"Привет, {user?.FirstName}! 👋\n\n" +
                                $"Твоя подписка на наш сервис закончилась 😭\n\n" +
                                $"Ты всегда можешь возобновить свою подписку и выбрать вариант подключения нажав /start");

                            logger.LogInformation("Конфиг пользователя удален, chatid: {chatid}", real_chatid);
                            count_deleted_users++;
                        }
                        catch (Exception ex) { logger.LogInformation("Ошибка в планировщике payment 0, exeption: {ex}", ex);}                      
                    }
                    await TgBotHostedService.bot.SendTextMessageAsync(1278048494, $"Всего должны продлить {count_continue} человек");
                    await TgBotHostedService.bot.SendTextMessageAsync(1278048494, $"Всего удалено {count_deleted_users} человек ({users_payment_0.Count})");
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
