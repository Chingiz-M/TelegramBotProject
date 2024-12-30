using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Globalization;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.Payments;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotProject.BaseClasses;
using TelegramBotProject.DB;
using TelegramBotProject.Entities;
using TelegramBotProject.Intarfaces;
using TelegramBotProject.TelegramBot;
using static TelegramBotProject.StartUp;

namespace TelegramBotProject.Services
{
    /// <summary>
    /// В этом классе выбираю на какои менно серваке создавать конфиг с помошью переменных Program.IPSEС_SELECTED_SERVER и Program.SOCKS_SELECTED_SERVER
    /// </summary>
    internal class BotComands : IBotCommands
    {
        private ILogger logger;
        private readonly IPSecServiceResolver ipsecResolver;
        private readonly SOCKSServiceResolver socksResolver;
        private readonly Comp_IPSecServiceResolver comp_ipsecResolver;

        //private readonly IIPsec1 ipsecServer;
        //private readonly ISocks1 socksServer;

        public BotComands(ILogger<BotComands> logger, IPSecServiceResolver ipsecResolver, SOCKSServiceResolver socksResolver, Comp_IPSecServiceResolver comp_ipsecResolver)
        {
            this.logger = logger;
            this.ipsecResolver = ipsecResolver;
            this.socksResolver = socksResolver;
            this.comp_ipsecResolver = comp_ipsecResolver;
            //ipsecServer = ipsecResolver(TgBotHostedService.IPSEС_SELECTED_SERVER); 
            //socksServer = socksResolver(TgBotHostedService.SOCKS_SELECTED_SERVER);
        }

        /// <summary>
        /// Метод начала РАБОТЫ БОТА и переход на типа устройства (комп или мобила)
        /// </summary>
        /// <param name="botClient"> бот клиент </param>
        /// <param name="chatid"> чат айди пользователя который голосует </param>
        /// <param name="connect"> тип подключения бесплтаный или платный </param>
        /// <returns></returns>
        public async Task BotStartBotSelectTypeDeviceAsync(ITelegramBotClient botClient, long chatid)
        {
            string startText = $"Привет тебе, Путник! 👋\n\n" +
                        $"84722 — это надежный и высокоскоростной VPN сервис с серверами в Нидерландах 🇳🇱, Израиле 🇮🇱 и Молдове 🇲🇩.\n\n" +
                        $"Мы предлагаем:\n" +
                        $"🚀 Высокую скорость;\n" +
                        $"🌗 Выбор из двух наиболее актуальных типов подключения для мобильных устройств;\n" +
                        $"🖥️ Отдельный сервер для ПК и просмотра YouTube в Full HD-качестве;\n" +
                        $"💳 Возможность оплаты картами РФ;\n" +
                        $"💰 Бесплатный пробный период на 2 недели и самую низкую цену на рынке!\n\n" +
                        $"Стоимость подписки после пробного периода составит:\n\n" +
                        $"от 83 ₽/мес для мобильных устройств,\n" +
                        $"от 99 ₽/мес для ПК и YouTube.\n" +
                        $"Для мобильных устройств действует реферальная система:\n" +
                        $"пригласите 3 друзей с активной подпиской и получите 3 месяца бесплатно.";

            var button1 = InlineKeyboardButton.WithCallbackData("Мобильное устройство 📱", NamesInlineButtons.StartMobile);
            var button2 = InlineKeyboardButton.WithCallbackData("Персональный компьютер 🖥️", NamesInlineButtons.StartComp);


            var row1 = new InlineKeyboardButton[] { button1, button2 };
            var keyboard = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup(row1);

            await botClient.SendTextMessageAsync(chatid, startText, replyMarkup: keyboard);
        }

        /// <summary>
        ///  Метод выбора периода оплаты и типа оплаты (продление, новый пользователь)
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="chatid"></param>
        /// <param name="typepayment"> тип оплаты (continue_pay или новый пользователь)</param>
        /// <returns></returns>
        public async Task BotSelectSendInvoiceAsync(ITelegramBotClient botClient, long chatid, string typepayment, string typedevice)
        {
            string device_info = typedevice == NamesInlineButtons.StartComp ? "Персональный компьютер" : "Мобильное устройство";

            InlineKeyboardButton? button1 = null;
            InlineKeyboardButton? button2 = null;

            if(typedevice == NamesInlineButtons.StartComp)
            {
                button1 = InlineKeyboardButton.WithCallbackData($"1 месяц за {TgBotHostedService.Price_1_Month_comp} ₽.", $"{typepayment}_{NamesInlineButtons.Month_1}");
                button2 = InlineKeyboardButton.WithCallbackData($"3 месяц за {TgBotHostedService.Price_3_Month_comp} ₽.", $"{typepayment}_{NamesInlineButtons.Month_3}");
            }
            else
            {
                button1 = InlineKeyboardButton.WithCallbackData($"1 месяц за {TgBotHostedService.Price_1_Month_mobile} ₽.", $"{typepayment}_{NamesInlineButtons.Month_1}");
                button2 = InlineKeyboardButton.WithCallbackData($"3 месяц за {TgBotHostedService.Price_3_Month_mobile} ₽.", $"{typepayment}_{NamesInlineButtons.Month_3}");
            }
            
            var row1 = new InlineKeyboardButton[] { button1, button2 };
            var keyboard = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup(row1);

            if (typepayment == NamesInlineButtons.ContinuePayment_Mobile) // продление подписки на мобильные устройства
                await botClient.SendTextMessageAsync(chatid, $"Выберите вариант продления подписки на наш сервис для мобильного устройства 📱.\n" +
                    $"({device_info})\n\n", replyMarkup: keyboard);   

            else if (typepayment == NamesInlineButtons.ContinuePayment_Comp) // продление подписки на ПК
                await botClient.SendTextMessageAsync(chatid, $"Выберите вариант продления подписки на наш сервис для ПК 💻.\n" +
                    $"({device_info})\n\n", replyMarkup: keyboard);

            else 
                await botClient.SendTextMessageAsync(chatid, $"Выберите вариант подписки на наш сервис 👀.\n" +
                    $"({device_info})\n\n" +
                    $"После оплаты 💳 вы получите конфигурационный файл и видеоинструкцию по его установке!\n\n" +
                    $"Процесс займет всего пару минут. 👌", replyMarkup: keyboard); // новые подключения
        }

        /// <summary>
        /// Посылаю инвойсы на оплату в зависимости от выбранной ос
        /// </summary>
        /// <param name="botClient"> бот клиент </param>
        /// <param name="chatid"> чат айди пользователя</param>
        /// <param name="typepayment"> тип оплаты либо продление либо ос которая была выбрана </param>
        /// <returns></returns>
        public async Task BotSendInvoiceAsync(ITelegramBotClient botClient, long chatid, string typepayment, int months, string typedevice)
        {
            int price = 0;
            string information_massage = null;
            string device_info = typedevice == NamesInlineButtons.StartComp ? "ПК" : "Мобильного устройства";

            switch (months)
            {
                case 1: 
                    price = TgBotHostedService.Price_1_Month_mobile;
                    information_massage = $"Оплата подписки на 1 месяц сервиса {TgBotHostedService.BotName} для {device_info}";
                    break;
                case 3:
                    price = TgBotHostedService.Price_3_Month_mobile;
                    information_massage = $"Оплата подписки на 3 месяца сервиса {TgBotHostedService.BotName} для {device_info}";
                    break;
                case 21:
                    price = TgBotHostedService.Price_1_Month_comp;
                    information_massage = $"Оплата подписки на 1 месяц сервиса {TgBotHostedService.BotName} для {device_info}";
                    break;
                case 23:
                    price = TgBotHostedService.Price_3_Month_comp;
                    information_massage = $"Оплата подписки на 3 месяца сервиса {TgBotHostedService.BotName} для {device_info}";
                    break;
                default:
                    break;
            }

            string jsonString = @"{""receipt"":{""customer"":{""email"":""mishaorlovorloff@yandex.ru""},""items"":[{""description"":""Оплата подписки 84722"",""quantity"":1.000,""amount"":{""value"":";
            jsonString += String.Format(@"""{0}"",", price.ToString("F2", CultureInfo.InvariantCulture));
            jsonString += @"""currency"":""RUB""},""vat_code"":1,""payment_mode"":""full_prepayment"",""payment_subject"":""commodity""}]}}";

            var list = new List<Telegram.Bot.Types.Payments.LabeledPrice>();
            list.Add(new Telegram.Bot.Types.Payments.LabeledPrice(information_massage, price * 100));
            await botClient.SendInvoiceAsync(
                chatid,
                "Подписка",
                information_massage,
                $"{typepayment}",
                TgBotHostedService.TokenPayment,
                "RUB",
                list,
                startParameter: "test",
                needName: false,
                needPhoneNumber: false,
                needEmail: false,
                isFlexible: false,
                providerData: jsonString);

            logger.LogInformation("Метод BotSendInvoiceAsync, посылаю инвойс, chatid: {chatid}, тип: {os}, подписка на месяцев {months}", chatid, typepayment, months);
        }

        /// <summary>
        /// Метод продления подписки активному пользователю (После оплаты см метод BotCheckStatusUserAndSendContinuePayInvoice => BotSendInvoiceAsync => BotContinuePaymentAsync)
        /// </summary>
        /// <param name="botClient"> бот клиент </param>
        /// <param name="chatID"> чат айди клиента </param>
        /// <param name="ProviderPaymentChargeId"> id оплаты пользователя </param>
        /// <returns></returns>
        public async Task BotContinuePaymentAsync(ITelegramBotClient botClient, long chatID, SuccessfulPayment? payment)
        {
            using (TgVpnbotContext db = new TgVpnbotContext())
            {
                UserDB? user = null;
                var CompChatID = chatID * TgBotHostedService.USERS_COMP;
                long info_chatid = 0;


                var match_ = Regex.Match(payment.InvoicePayload, @"continue_pay_(?<typedev>.*)_(?<period>\d+_.*)");

                if (match_.Success)
                {
                    // Mobile
                    if (match_.Groups["typedev"].ToString() == NamesInlineButtons.StartMobile)
                    {
                        user = await db.Users.FirstOrDefaultAsync(u => u.ChatID == chatID);
                        info_chatid = chatID;
                    }
                    // Comp
                    else if (match_.Groups["typedev"].ToString() == NamesInlineButtons.StartComp)
                    {
                        user = await db.Users.FirstOrDefaultAsync(u => u.ChatID == CompChatID);
                        info_chatid = CompChatID;
                    }

                }
              
                if (user != null) // мб только активный юзер (неактивный не сможет нажать и оплатить по continue_pay)
                {
                    if (match_.Success)
                    {
                        // 1 MONTH
                        if (match_.Groups["period"].ToString() == NamesInlineButtons.Month_1)
                            user.DateNextPayment = user.DateNextPayment.AddMonths(1);
                        // 3 MONTH
                        else if (match_.Groups["period"].ToString() == NamesInlineButtons.Month_3)
                            user.DateNextPayment = user.DateNextPayment.AddMonths(3);
                        
                        user.ProviderPaymentChargeId = payment?.ProviderPaymentChargeId;

                        db.Users.Update(user);
                        await db.SaveChangesAsync();

                        await botClient.SendTextMessageAsync(chatID, $"Оплата прошла успешно! ✅\n\n" +
                            $"Ваш ID = " + chatID.ToString() + $". Подписка активна до {user.DateNextPayment.ToString("dd-MM-yyyy")} г.\n\n" +
                            $"Не удаляйте и не останавливайте бота, ближе к концу периода здесь появится новая ссылка оплаты, если вы решите остаться с нами. 🤗\n\n" +
                            $"*ID необходим для участия в реферальной программе и отслеживания периода оплаты. 😉");

                        await botClient.SendTextMessageAsync(1278048494, $"Пользователь {info_chatid} продлил подписку"); // присылаю себе ответ по продлению
                    }                   

                    logger.LogInformation("Метод BotContinuePaymentAsync, пользователь продлил оплату, chatid: {chatid}, paymentID: {payID} ", chatID, payment?.ProviderPaymentChargeId);
                }
                else
                {
                    await botClient.SendTextMessageAsync(chatID, $"Странно, вас нет в нашей базе данных 🤔\n" +
                        $"Мы решим этот вопрос, обратитесь в поддержку /support 🥸");

                    logger.LogInformation("Метод BotContinuePaymentAsync, нет пользователя в БД, продление оплаты, chatid: {chatid} ", chatID);
                }
            }
        }

        /// <summary>
        /// Метод добавления пользователей для рефераальной программы
        /// </summary>
        /// <param name="botClient"> бот клиент </param>
        /// <param name="chatid"> чат айди пользователя который голосует </param>
        /// <param name="clientID"> чат айди пользователя за которого голосуют</param>
        /// <returns></returns>
        public async Task BotAddReferalProgramAsync(ITelegramBotClient botClient, long chatid, long clientID)
        {
            using (TgVpnbotContext db = new TgVpnbotContext())
            {
                var ref_user = await db.Users.FirstOrDefaultAsync(u => u.ChatID == chatid && u.Status == "active"); // проверяю есть ли такой активный пользователь и может ли голосовать
                if (ref_user != null)
                {
                    if (ref_user.ProviderPaymentChargeId == NamesInlineButtons.Mobile_TryFreePeriod)
                    {
                        await botClient.SendTextMessageAsync(chatid, $"Увы, реферальная программа не действует на пробном периоде 😢");
                        return;
                    }
                    if (ref_user.ReferalVoice != 0) // если пользователь уже голосовал по рефералке
                    {
                        await botClient.SendTextMessageAsync(chatid, $"Увы, вы уже голосовали за друга c chatID = {ref_user.ReferalVoice}");
                    }
                    else // если еще не голосовал
                    {
                        var user = await db.Users.FirstOrDefaultAsync(u => u.ChatID == clientID && u.Status == "active"); // пользователь за которого голосуют
                        if (user != null)
                        {
                            if (clientID == chatid) // когда голосуют за себя (нельзя)
                            {
                                await botClient.SendTextMessageAsync(chatid, $"Увы, за себя голосовать нелья 😇");
                                return;
                            }

                            user.Referal++; // добавляю 1 голос позвавшему
                            ref_user.ReferalVoice = clientID; // добавляю chatid голос позванному
                            await db.SaveChangesAsync();

                            await botClient.SendTextMessageAsync(chatid, $"Пользователю с chatID: {clientID} добавлен 1 голос от вас 🥳");
                            await botClient.SendTextMessageAsync(clientID, $"Пользователь с chatID: {chatid} проголосовал за вас, теперь вы его пригласитель 🥳");

                            user = await db.Users.FirstOrDefaultAsync(u => u.ChatID == clientID && u.Status == "active");

                            if (user.Referal % 3 == 0) // за каждых 3-х друзей +3 месяца подписки
                            {
                                await botClient.SendTextMessageAsync(clientID, $"Вы привели уже {user.Referal} друзей и получаете 3 месяца бесплатного пользования сервисом! 🎉");

                                user.DateNextPayment = user.DateNextPayment.AddMonths(3);
                                await db.SaveChangesAsync();

                                await botClient.SendTextMessageAsync(clientID, $"Ваша подписка активна до {user.DateNextPayment.AddMonths(3).ToString("dd-MM-yyyy")}");

                            }
                            else
                            {
                                if (user.Referal == 1)
                                {
                                    await botClient.SendTextMessageAsync(clientID, $"Вы привели уже {user.Referal} друга! 🎉\n" +
                                    $"За каждых 3-х друзей вы получаете 3 месяца бесплатного пользования нашим сервисом 😎");
                                }
                                else
                                {
                                    await botClient.SendTextMessageAsync(clientID, $"Вы привели уже {user.Referal} друзей! 🎉\n" +
                                        $"За каждых 3-х друзей вы получаете 3 месяца бесплатного пользования нашим сервисом 😎");
                                }
                            }
                            logger.LogInformation("Метод BotAddReferalProgramAsync, голосует chatid: {chatid}, за кого голосует chatid: {clientID}, ОС: {os} ", chatid, clientID);
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(chatid, $"Активного пользователя с таким chatID нет в базе данных 🥺\n" +
                                $"Проверьте правильность введенного chatID 🫣");
                        }
                    }
                }
                else // если пользователь который голосует не является активным
                {
                    await botClient.SendTextMessageAsync(chatid, $"Увы, вы пока не являетесь активным пользователем нашего сервиса 🫣\n" +
                        $"Нажмите /start для начала работы! 😎");
                }
            }
        }

        /// <summary>
        /// Метод проверки пользователя активный или нет и если он нажал кнопку продлеия подписки continue_pay
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="chatID"></param>
        /// <param name="typepayment"></param>
        /// <param name="months"></param>
        /// <returns></returns>
        public async Task BotCheckStatusUserAndSendContinuePayInvoice(ITelegramBotClient botClient, long chatID, string typepayment, int months, string typedevice)
        {
            using (TgVpnbotContext db = new TgVpnbotContext())
            {
                var CompChatID = chatID * TgBotHostedService.USERS_COMP;
                var info_chatid = typedevice == NamesInlineButtons.StartComp ? CompChatID : chatID;

                var user = await db.Users.FirstOrDefaultAsync(u => u.ChatID == info_chatid).ConfigureAwait(false);

                if (user != null)
                {
                    if (user.Status == "active") // отправляю инвойс на продление подписик уже активным пользователям
                    {
                        if (typedevice == NamesInlineButtons.StartComp)
                            await BotSendInvoiceAsync(botClient, chatID, typepayment, months, NamesInlineButtons.StartComp);

                        else if (typedevice == NamesInlineButtons.StartMobile)
                            await BotSendInvoiceAsync(botClient, chatID, typepayment, months, NamesInlineButtons.StartMobile);

                    }

                    else if (user.Status == "nonactive") // когда пользователь есть в БД и он не активный и он нажал на continue_pay кнопку а не новую 
                        await botClient.SendTextMessageAsync(chatID,
                            $"Твоя подписка на наш сервис закончилась 😭\n\n" +
                            $"Ты всегда можешь возобновить свою подписку и выбрать вариант подключения нажав /start");
                }
            
            }
        }

        /// <summary>
        /// Метод проверки и использования промокода пользователем
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="chatID"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task BotCheckAndUsePromoAsync(ITelegramBotClient botClient, long chatID)
        {
            using (TgVpnbotContext db = new TgVpnbotContext())
            {
                var user = await db.Users.FirstOrDefaultAsync(u => u.ChatID == chatID).ConfigureAwait(false);

                if (user != null)
                {
                    if(user.UsePromocode)
                        await botClient.SendTextMessageAsync(chatID, $"Ты уже использовал этот промокод 👀");
                    else
                    {
                        user.UsePromocode = true;
                        user.DateNextPayment = user.DateNextPayment.AddMonths(1); // добавляю месяц за введенный промокод
                        await db.SaveChangesAsync();

                        await botClient.SendTextMessageAsync(chatID, $"Поздравляю! 🎉\nПромокод успешно применен! \n\n" +
                            $"Подписка активна до {user.DateNextPayment.ToString("dd-MM-yyyy")} г. включительно 🤝\n");

                    }
                }
            }
        }

        /// <summary>
        /// Метод который убирает галки об использовании промокода в бд всем пользователям
        /// </summary>
        /// <param name="botClient"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task BotTakeOffDBPromoAsync(ITelegramBotClient botClient, long chatID)
        {
            using (TgVpnbotContext db = new TgVpnbotContext())
            {
                List<UserDB> users = new List<UserDB>();
                users = await db.Users.Where(u => u.UsePromocode == true).ToListAsync();

                foreach(var user in users)
                {
                    user.UsePromocode = false;
                }

                await botClient.SendTextMessageAsync(chatID, $"Галки убраны у {users.Count} пользователей");
                await botClient.SendTextMessageAsync(1278048494, $"Галки убраны у {users.Count} пользователей");

                await db.SaveChangesAsync();

            }
        }

        /// <summary>
        /// Метод проверки и использования промокода пользователем
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="chatID"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task BotMarkBlatnoiAsync(ITelegramBotClient botClient, long chatID, long adminID)
        {
            using (TgVpnbotContext db = new TgVpnbotContext())
            {
                var user = await db.Users.FirstOrDefaultAsync(u => u.ChatID == chatID).ConfigureAwait(false);

                if (user != null)
                {
                    if(user.Blatnoi)
                        user.Blatnoi = false;
                    else
                        user.Blatnoi = true;

                    await db.SaveChangesAsync();

                    await botClient.SendTextMessageAsync(1278048494, $"Сделал пользователя {chatID} блатным: {user.Blatnoi}");
                    await botClient.SendTextMessageAsync(adminID, $"Сделал пользователя {chatID} блатным: {user.Blatnoi}");
                }
            }
        }

        /// <summary>
        /// Метод добавления дней к активной подписке определенному пользователю
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="chatID"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task BotAddDaysToUserAsync(ITelegramBotClient botClient, long chatID, int days, long adminID)
        {
            using (TgVpnbotContext db = new TgVpnbotContext())
            {
                var user = await db.Users.FirstOrDefaultAsync(u => u.ChatID == chatID).ConfigureAwait(false);

                if (user != null)
                {
                    user.DateNextPayment = user.DateNextPayment.AddDays(days);
                    await db.SaveChangesAsync();

                    await botClient.SendTextMessageAsync(1278048494, $"Добавил пользователю {chatID} {days} дней");
                    await botClient.SendTextMessageAsync(adminID, $"Добавил пользователю {chatID} {days} дней");
                }
            }
        }

        /// <summary>
        ///  проверка есть ли в бд юзер
        /// </summary>
        /// <param name="chatID"></param>
        /// <param name="serachType"> тип поиска 1 - АКТИВНОГО пользователя 2 - просто НАЛИЧИЕ в бд </param>
        /// <returns></returns>
        public async Task<UserDB> BotCheckUserBDAsync(long chatID, int serachType)
        {
            UserDB? usr = null;
            using (TgVpnbotContext db = new TgVpnbotContext())
            {
                if (serachType == 1)
                    usr = await db.Users.FirstOrDefaultAsync(u => u.ChatID == chatID && u.Status == "active").ConfigureAwait(false);
                else
                    usr = await db.Users.FirstOrDefaultAsync(u => u.ChatID == chatID).ConfigureAwait(false);

            }
            return usr;
        }

        /// <summary>
        /// Вспомогательный метод для получения всех ID пользователей из бд и отправке им сообщения
        /// </summary>
        /// <param name="typeService"></param>
        /// <returns></returns>
        public async Task<int> BotSendMessageToUsersInDBAsync(ITelegramBotClient botClient, string typeService, string text, int numServer)
        {
            List<long> listID = new List<long>();
            int count = 0;
            using (TgVpnbotContext db = new TgVpnbotContext())
            {
                if (typeService == NamesInlineButtons.IPSEC_android)
                {
                    listID = await db.Users
                    .Where(u => u.Status == "active" && u.NameOS == NamesInlineButtons.Android && EF.Functions.Like(u.NameService, $"{NamesInlineButtons.IPSEC}%"))
                    .Select(user => user.ChatID).ToListAsync().ConfigureAwait(false);
                }
                if (typeService == NamesInlineButtons.IPSEC_ios)
                {
                    listID = await db.Users
                    .Where(u => u.Status == "active" && u.NameOS == NamesInlineButtons.IOS && EF.Functions.Like(u.NameService, $"{NamesInlineButtons.IPSEC}%"))
                    .Select(user => user.ChatID).ToListAsync().ConfigureAwait(false);
                }
                if (typeService == NamesInlineButtons.Socks)
                {
                    listID = await db.Users
                    .Where(u => u.Status == "active" && EF.Functions.Like(u.NameService, $"{NamesInlineButtons.Socks}%"))
                    .Select(user => user.ChatID).ToListAsync().ConfigureAwait(false);
                }
                if (typeService == NamesInlineButtons.AllUsers)
                {
                    listID = await db.Users
                    .Where(u => u.Status == "active")
                    .Select(user => user.ChatID).ToListAsync().ConfigureAwait(false);
                }
                if (typeService == NamesInlineButtons.StartComp)
                {
                    listID = await db.Users
                    .Where(u => u.Status == "active" && u.TypeOfDevice == NamesInlineButtons.StartComp)
                    .Select(user => user.ChatID).ToListAsync().ConfigureAwait(false);
                }
                if (typeService == "nonactive")
                {
                    listID = await db.Users
                    .Where(u => u.Status == "nonactive")
                    .Select(user => user.ChatID).ToListAsync().ConfigureAwait(false);
                }
                if (typeService == "server_ipsec")
                {
                    listID = await db.Users
                    .Where(u => u.Status == "active" && u.NameService == $"IPSEC_{numServer}")
                    .Select(user => user.ChatID).ToListAsync().ConfigureAwait(false);
                }
                if (typeService == "server_socks")
                {
                    listID = await db.Users
                    .Where(u => u.Status == "active" && u.NameService == $"SOCKS_{numServer}")
                    .Select(user => user.ChatID).ToListAsync().ConfigureAwait(false);
                }
            }

            List<long> testBlock = new List<long>();
            foreach (var uID in listID)
            {
                try
                {
                    await botClient.SendTextMessageAsync(uID, text);
                    count++;
                }
                catch (Exception ex)
                {
                    testBlock.Add(uID);
                    logger.LogInformation("BotSendMessageToUsersInDBAsync {ex}", ex);
                }
            }

            return count;

        }

        /// <summary>
        /// Специфичный метод бесплатного продления пользователей в ближайшие дни (упал прием оплаты картами)
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="diapason"> диапазон ближайших дат для выбора начиная от сегодняшней</param>
        /// <param name="dayslong"> кол-во дней на которые продлить подписки пользователей в диапазоне дат</param>
        /// <param name="adminID"></param>
        /// <returns></returns>
        public async Task BotShiftNearestUsersDateAsync(ITelegramBotClient botClient, int diapason, int dayslong, long adminID)
        {
            using (TgVpnbotContext db = new TgVpnbotContext())
            {
                var users_payment = await db.Users.Where(u => u.DateNextPayment.Date < DateTime.Now.AddDays(diapason).Date && u.Status == "active" && u.Blatnoi == false).ToListAsync().ConfigureAwait(false);

                int count = 0;
                foreach (var user in users_payment) // здесь все по датам включая chatid для компа и для телефонов
                {
                    try
                    {
                        user.DateNextPayment = user.DateNextPayment.AddDays(dayslong);
                        await db.SaveChangesAsync();
                        count++;
                        
                    }
                    catch (Exception ex) {  }
                }
                await botClient.SendTextMessageAsync(1278048494, $"Продлил {count} ({users_payment.Count}) пользователей в диапазоне {diapason} дней на {dayslong} дней");
                await botClient.SendTextMessageAsync(adminID, $"Продлил {count} ({users_payment.Count}) пользователей в диапазоне {diapason} дней на {dayslong} дней");
            }
        }

        /// <summary>
        /// Метод получения общего кол-ва пользователей в БД и по факту на серваках
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="adminID"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task BotGetAllCountUsersAsync(ITelegramBotClient botClient, long adminID)
        {
            using (TgVpnbotContext db = new TgVpnbotContext())
            {
                // подсчет пользователей в бд
                var users_all_active_db = await db.Users.CountAsync(u => u.Status == "active").ConfigureAwait(false);
                var users_all_non = await db.Users.CountAsync(u => u.Status == "nonactive").ConfigureAwait(false);
                var users_blatnoi = await db.Users.CountAsync(u => u.Status == "active" && u.Blatnoi == true).ConfigureAwait(false);

                var users_ipsec_db = await db.Users.CountAsync(u => u.Status == "active" && u.Blatnoi == false && EF.Functions.Like(u.NameService, "%ipsec%")).ConfigureAwait(false);

                var users_ipsec_mob = await db.Users.
                    CountAsync(u => u.Status == "active" && u.Blatnoi == false && EF.Functions.Like(u.NameService, "ipsec%") 
                    && EF.Functions.Like(u.TypeOfDevice, "mobile%")).ConfigureAwait(false);

                var users_ipsec_mob_ios = await db.Users.
                    CountAsync(u => u.Status == "active" && u.Blatnoi == false && EF.Functions.Like(u.NameService, "ipsec%") 
                    && EF.Functions.Like(u.TypeOfDevice, "mobile%") && u.NameOS == "ios").ConfigureAwait(false);

                var users_ipsec_mob_android = await db.Users.
                    CountAsync(u => u.Status == "active" && u.Blatnoi == false && EF.Functions.Like(u.NameService, "ipsec%") 
                    && EF.Functions.Like(u.TypeOfDevice, "mobile%") && u.NameOS == "android").ConfigureAwait(false);

                var users_ipsec_comp = await db.Users.
                    CountAsync(u => u.Status == "active" && u.Blatnoi == false && EF.Functions.Like(u.NameService, "comp%") 
                    && EF.Functions.Like(u.TypeOfDevice, "comp%")).ConfigureAwait(false);

                var users_ipsec_comp_win = await db.Users.
                    CountAsync(u => u.Status == "active" && u.Blatnoi == false && EF.Functions.Like(u.NameService, "comp%") 
                    && EF.Functions.Like(u.TypeOfDevice, "comp%") && u.NameOS == "Windows").ConfigureAwait(false);

                var users_ipsec_comp_mac = await db.Users.
                    CountAsync(u => u.Status == "active" && u.Blatnoi == false && EF.Functions.Like(u.NameService, "comp%") 
                    && EF.Functions.Like(u.TypeOfDevice, "comp%") && u.NameOS == "MacOS").ConfigureAwait(false);

                var users_socks = await db.Users.CountAsync(u => u.Status == "active" && u.Blatnoi == false && EF.Functions.Like(u.NameService, "socks%")).ConfigureAwait(false);
                var users_socks_ios = await db.Users.
                   CountAsync(u => u.Status == "active" && u.Blatnoi == false && EF.Functions.Like(u.NameService, "socks%") && u.NameOS == "ios").ConfigureAwait(false);

                var users_socks_aandroid = await db.Users.
                   CountAsync(u => u.Status == "active" && u.Blatnoi == false && EF.Functions.Like(u.NameService, "socks%") && u.NameOS == "android").ConfigureAwait(false);

                await botClient.SendTextMessageAsync(adminID, $"Данные с БД\n\n" +
                    $"Всего активных в БД: {users_all_active_db}\n" +
                    $"Всего неактивных в БД: {users_all_non}\n" +
                    $"Всего блатных в БД: {users_blatnoi}\n\n" +
                    $"Всего IPSEC (мобилы и компы) в БД: {users_ipsec_db}\n" +
                    $"IPSEC мобилы в БД: {users_ipsec_mob}\n" +
                    $" -IPSEC мобилы ios в БД: {users_ipsec_mob_ios}\n" +
                    $" -IPSEC мобилы android в БД: {users_ipsec_mob_android}\n" +
                    $"IPSEC комп в БД: {users_ipsec_comp}\n" +
                    $" -IPSEC комп Windows в БД: {users_ipsec_comp_win}\n" +
                    $" -IPSEC комп MacOS в БД: {users_ipsec_comp_mac}\n\n" +
                    $"Всего Socks в БД: {users_socks}\n" +
                    $" -Socks ios в БД: {users_socks_ios}\n" +
                    $" -Socks android в БД: {users_socks_aandroid}\n\n");

            }

            // подсчет фактических конфигов на серваках
            int users_all_servers = 0;
            int users_all_ipsec_mob_servers = 0;
            foreach (var item in TgBotHostedService.IPSEC_SERVERS_LIST)
            {
                var temp_server = ipsecResolver(item);
                var count_users = await temp_server.GetTotalUserAsync();
                users_all_ipsec_mob_servers += count_users;
            }

            int users_all_ipsec_comp_servers = 0;
            foreach (var item in TgBotHostedService.Comp_IPSEC_SERVERS_LIST)
            {
                var temp_server = comp_ipsecResolver(item);
                var count_users = await temp_server.GetTotalUserAsync();
                users_all_ipsec_comp_servers += count_users;
            }

            int users_all_socks_servers = 0;
            foreach (var item in TgBotHostedService.SOCKS_SERVERS_LIST)
            {
                var temp_server = socksResolver(item);
                var count_users = await temp_server.GetFileUsersAsync("test", false);
                users_all_socks_servers += count_users;
            }

            users_all_servers = users_all_ipsec_mob_servers + users_all_ipsec_comp_servers + users_all_socks_servers;

            await botClient.SendTextMessageAsync(adminID, $"Фактические данные с серверов\n\n" +
                $"Всего Socks и IPSEC (моб и комп): {users_all_servers}\n" +
                $"Всего IPSEC мобилы: {users_all_ipsec_mob_servers}\n" +
                $"Всего IPSEC комп: {users_all_ipsec_comp_servers}\n" +
                $"Всего Socks в БД: {users_all_socks_servers}\n");
        }
    }
}
