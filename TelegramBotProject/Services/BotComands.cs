using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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
        #region Instructions

        private string IosInstructionIPSec = "Инструкция\n\n" +
                        "1. Зайти в 'Настройки' в раздел 'VPN' и удалить все старые VPN подключения\n" +
                        "2. Скачать конфигурационный файл *.mobileconfig в 'Файлы' iPhone\n" +
                        "3. Зайти в приложение 'Файлы', найти файл и нажать на него => появится уведомление, что профиль загружен\n" +
                        "4. Зайти в настройки, там под верхней табличкой профиля Apple ID появится новая вкладка, в которой Вас ждет запрос на установку профиля VPN\n" +
                        "5. Зайти в эту вкладку, согласиться всеми терминами и нажать 'Установить'\n" +
                        "6. Теперь Вы сможете просто ползунком в настройках включать/выключать VPN";

        private string AndroidInstructionIPSec = "Инструкция\n\n" +
                        "1. Скачать конфигурационный файл *.p12\n" +
                        "2. В приложении 'Проводник' найти ваш файл *.p12 (Возможные пути расположения загруженного файла: /Downloads, /Downloads/Telegram, /Telegram/Telegram_files)\n" +
                        "3. После нажатия на этот файл появится окно установки, нажмите 'Ок'\n" +
                        "4. Перейти в GooglePlay, установить приложение 'strongSwan VPN Client'\n" +
                        "5. Зайти в скачанное приложение, нажать на красное поле 'Добавить VPN профиль' вверху\n" +
                        "6. Прописать сервер (адрес укажем ниже)\n" +
                        "7. Выбрать тип VPN подключения: 'IKEv2 Сертификат'\n" +
                        "8. В поле 'Выбор сертификата' выберите имя своего сертификата *.p12 в выпадающем окне (если появляется требование 'введите пароль от хранилища учетных данных' => установите пин - код блокировки телефона в настройках)\n" +
                        "9. Сертификат CA оставьте по - умолчанию(выбрать автоматически)'\n" +
                        "10. Нажмите 'Сохранить' в красном поле вверху\n" +
                        "11. В главным меню приложения strongSwan выберите появившееся подключение, со всем согласитесь\n" +
                        "12. Готово, включать и выключать VPN по желанию можно в приложении !";

        private string IosInstructionSocks = "Инструкция\n\n" +
                        "1.Установите приложение \"Outline App\" из Google Play.\n" +
                        "2. Полученный от бота ключ вида 'ss://.../?outline=1' скопируйте в Outline: для этого нажмите на '+' в правом верхнем углу главного экрана приложения и поместите ваш ключ в появившееся поле.Outline автоматически обработает ключ.\n" +
                        "3. Нажмите на кнопку \"Подключить\", cерый кружок в приложении позеленеет.Cоединение установлено!\n" +
                        "4. Готово, включать и выключать Outline необходимо в приложении";

        private string AndroidInstructionSocks = "Инструкция\n\n" +
                        "1.Установите приложение \"Outline App\" из App Store.\n" +
                        "2. Полученный от бота ключ вида 'ss://.../?outline=1' скопируйте в Outline: для этого нажмите на '+' в правом верхнем углу главного экрана приложения и поместите ваш ключ в появившееся поле.Outline автоматически обработает ключ.\n" +
                        "3. Нажмите на кнопку \"Подключить\", cерый кружок в приложении позеленеет.Cоединение установлено!\n" +
                        "4. Готово, включать и выключать Outline необходимо в приложении";

        private string StartText = $"Привет тебе, Путник! 👋\n\n" +
                        $"84722 — это надежный и высокоскоростной VPN сервис с серверами в Нидерландах 🇳🇱 и Израиле 🇮🇱.\n\n" +
                        $"Мы предлагаем:\n" +
                        $"🚀 Высокую скорость\n" +
                        $"🌗 Выбор из двух наиболее актуальных типов подключения\n" +
                        $"💳 Возможность оплаты картами РФ\n" +
                        $"💰 Бесплатный пробный период в 2 недели и самую низкую цену на рынке!\n\n" +
                        $"Стоимость подписки после пробного периода составит 99 ₽/мес.\n\n" +
                        $"Действует реферальная система: пригласите 3 друзей в наш сервис и получите 3 месяца бесплатно.";
        #endregion


        private ILogger logger;
        private readonly IPSecServiceResolver ipsecResolver;
        private readonly SOCKSServiceResolver socksResolver;
        //private readonly IIPsec1 ipsecServer;
        //private readonly ISocks1 socksServer;

        public BotComands(ILogger<BotComands> logger, IPSecServiceResolver ipsecResolver, SOCKSServiceResolver socksResolver)
        {
            this.logger = logger;
            this.ipsecResolver = ipsecResolver;
            this.socksResolver = socksResolver;
            //ipsecServer = ipsecResolver(TgBotHostedService.IPSEС_SELECTED_SERVER); 
            //socksServer = socksResolver(TgBotHostedService.SOCKS_SELECTED_SERVER);
        }

        /// <summary>
        /// Балансер конфигов IPsec, ищет где меньше 100 конфигов и отпрляет имя сервака
        /// </summary>
        /// <returns></returns>
        public async Task<string> BotIPsecConfigBalanserAsync()
        { 
            foreach (var item in TgBotHostedService.IPSEC_SERVERS_LIST)
            {
                if(item != "IPSEC_1")
                {
                    var temp_server = ipsecResolver(item);
                    var count_users = await temp_server.GetTotalUserAsync();
                    if (count_users < TgBotHostedService.CountINServerIpSec)
                        return item;
                }               
            }

            return TgBotHostedService.IPSEC_SERVERS_LIST[0];
        }

        /// <summary>
        /// Балансер ключей носков, ищет сервак где меньше 100 и отправляет имя
        /// </summary>
        /// <returns></returns>
        public async Task<string> BotSocksKeyBalanserAsync()
        {
            foreach (var item in TgBotHostedService.SOCKS_SERVERS_LIST)
            {
                var temp_server = socksResolver(item);
                var count_users = await temp_server.GetFileUsersAsync("test", false);
                if (count_users < TgBotHostedService.CountINServerSocks)
                    return item;
            }

            return TgBotHostedService.SOCKS_SERVERS_LIST[0];
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Метод начала РАБОТЫ БОТА и переход на типа устройства (комп или мобила)
        /// </summary>
        /// <param name="botClient"> бот клиент </param>
        /// <param name="chatid"> чат айди пользователя который голосует </param>
        /// <param name="connect"> тип подключения бесплтаный или платный </param>
        /// <returns></returns>
        public async Task BotStartBotSelectTypeDeviceAsync(ITelegramBotClient botClient, long chatid)
        {
            string startText = $"Пожалуйста, выбери тип подключения:\n\n" +
                $"Мобила (нужен текст)\n\n" +
                $"Комп (нужен текст)\n\n";

            var button1 = InlineKeyboardButton.WithCallbackData("Мобила", NamesInlineButtons.StartMobile);
            var button2 = InlineKeyboardButton.WithCallbackData("Комп", NamesInlineButtons.StartComp);


            var row1 = new InlineKeyboardButton[] { button1, button2 };
            var keyboard = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup(row1);

            await botClient.SendTextMessageAsync(chatid, startText, replyMarkup: keyboard);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="chatid"></param>
        /// <returns></returns>
        public async Task BotStartAsync(ITelegramBotClient botClient, long chatid)
        {
            var user_mobile = await BotCheckUserBDAsync(chatid, 2); // есть ли ваще

            if (user_mobile == null) // если нажал старт новый пользователь которого нет в бд (если нет хотя бы одного или компа или мобилы)
            {
                var button = InlineKeyboardButton.WithCallbackData("🔥 Попробовать бесплатный период 🔥", NamesInlineButtons.Mobile_TryFreePeriod);
                var row = new InlineKeyboardButton[] { button };
                var keyboard = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup(row);

                await botClient.SendTextMessageAsync(chatid, StartText, replyMarkup: keyboard);
            }
            else // пользователь нажал старт и он есть в бд
            {
                string startText = $"Привет тебе, Путник!👋 Рады видеть тебя снова!\n\n" +
                    $"Ты использовал свой пробный период. Мы надеемся, что не разочаровали. 🙌\n" +
                    $"Чтобы продолжить пользоваться сервисом, необходимо приобрести подписку.\n\n" +
                    $"Стоимость подписки после пробного периода составит 99 ₽/ мес.";
                await botClient.SendTextMessageAsync(chatid, startText);

                await BotSelectServiceAsync(botClient, chatid, TgBotHostedService.TypeConnect.Payment);
            }

        }     

        /// <summary>
        /// Метод выбор сервиса и начало пробного либо платного периодов
        /// </summary>
        /// <param name="botClient"> бот клиент </param>
        /// <param name="chatid"> чат айди пользователя который голосует </param>
        /// <param name="connect"> тип подключения бесплтаный или платный </param>
        /// <returns></returns>
        public async Task BotSelectServiceAsync(ITelegramBotClient botClient, long chatid, TgBotHostedService.TypeConnect connect)
        {
            string startText = $"Пожалуйста, выбери тип подключения:\n\n" +
                $"🔹IPSec(IKEv2) — классический и самый быстрый мобильный протокол c шифрованием трафика — не нагружает устройство, напрямую поддерживается iOS, требует установки специального клиента на Android.\n\n" +
                $"🔸Shadowsocks(Outline) — современный, наиболее стабильный и надёжный протокол, основанный на маскирующем прокси SOCKS5 — минимально нагружает устройство, требует установки специального клиента с простейшей настройкой.\n\n";

            InlineKeyboardButton? button1 = null;
            InlineKeyboardButton? button2 = null;

            
            switch (connect)
            {
                case TgBotHostedService.TypeConnect.Free:

                    button1 = InlineKeyboardButton.WithCallbackData("IPSec(IKEv2) 🔴", NamesInlineButtons.Mobile_TryFreePeriod_IpSec);
                    button2 = InlineKeyboardButton.WithCallbackData("Shadowsocks(Outline) 🔵", NamesInlineButtons.Mobile_TryFreePeriod_Socks);
                    startText += "(Бесплатный период)";

                    break;
                case TgBotHostedService.TypeConnect.Payment:

                    button1 = InlineKeyboardButton.WithCallbackData("IPSec(IKEv2) 🔴", NamesInlineButtons.StartIPSEC);
                    button2 = InlineKeyboardButton.WithCallbackData("Shadowsocks(Outline) 🔵", NamesInlineButtons.StartSocks);

                    break;
                default:
                    break;
            }
            
            var row1 = new InlineKeyboardButton[] { button1, button2 };
            var keyboard = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup(row1);

            await botClient.SendTextMessageAsync(chatid, startText, replyMarkup: keyboard);
        }

        /// <summary>
        /// Метод старта бота
        /// </summary>
        /// <param name="botClient"> бот клиент </param>
        /// <param name="chatid"> чат айди клиента </param>
        /// <param name="FirstName"> имя клиента если есть</param>
        /// <param name="ServiceName"> имя сервиса что бы добавить в payload для идентификации inline кнопки</param>
        /// <returns></returns>
        public async Task BotSelectOpSysAsync(ITelegramBotClient botClient, long chatid, string? FirstName, string ServiceName)
        {
            var button1 = InlineKeyboardButton.WithCallbackData("ios 📱", $"{ServiceName}_{NamesInlineButtons.IOS}");
            var button2 = InlineKeyboardButton.WithCallbackData("android 🤖", $"{ServiceName}_{NamesInlineButtons.Android}");
            var row1 = new InlineKeyboardButton[] { button1, button2 };
            var keyboard = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup(row1);

            var s_name = string.Empty;
            if (ServiceName == NamesInlineButtons.StartIPSEC || ServiceName == NamesInlineButtons.Mobile_TryFreePeriod_IpSec)
                s_name = "IPSec(IKEV2)";
            else
                s_name = "Shadowsocks(Outline)";

            await botClient.SendTextMessageAsync(chatid, $"Ты выбрал {s_name} 👏\n" +
                $"Теперь выбери свою операционную систему. 👀", replyMarkup: keyboard);

        }

        /// <summary>
        /// Метод начала пробного периода
        /// Создание конфига и занесение пользователя в БД 
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="button"></param>
        /// <param name="typeconnect"></param>
        /// <returns></returns>
        public async Task BotBeginFreePeriodAsync(ITelegramBotClient botClient, Telegram.Bot.Types.CallbackQuery button, string typeconnect)
        {
            using (TgVpnbotContext db = new TgVpnbotContext())
            {
                var chatID = button.Message.Chat.Id;
                var user = await db.Users.FirstOrDefaultAsync(u => u.ChatID == chatID).ConfigureAwait(false);

                if (user != null) // если пользователь уже есть но его быть не должно
                {
                    await botClient.SendTextMessageAsync(1278048494, $"ERROR Метод BotBeginFreePeriodAsync пользователь id {chatID} есть в бд но не должно быть его"); // присылаю себе

                    logger.LogInformation("Метод BotBeginFreePeriodAsync пользователь id {chatID} есть в бд но не должно быть его", chatID);
                }
                else // если новый пользователь
                {
                    user = new UserDB
                    {
                        FirstName = button.Message.Chat?.FirstName,
                        Username = button.Message.Chat?.Username,
                        ChatID = chatID,
                        ProviderPaymentChargeId = NamesInlineButtons.Mobile_TryFreePeriod,
                        TypeOfDevice = NamesInlineButtons.StartMobile,
                    };

                    await botClient.SendTextMessageAsync(chatID, $"Бесплатный период начался! ✅\n\n" +
                        $"Ваш ID = " + chatID.ToString() + $". Подписка активна до {user.DateNextPayment.ToString("dd-MM-yyyy")} г. включительно\n\n" +
                        $"Не удаляйте и не останавливайте бота, ближе к концу периода здесь появится ссылка оплаты, если вы решите остаться с нами. 🤗\n\n" +
                        $"*ID необходим для участия в реферальной программе и отслеживания периода оплаты. 😉");


                    IIPsec1 ipsecServer = null;
                    (ISocks1 socksServer, long id_newUser) resFuncSocksServer = (null, 0);

                    // IOS
                    if (typeconnect == NamesInlineButtons.Mobile_TryFreePeriod_IpSec_ios)
                    {
                        ipsecServer = await CreateAndSendConfig_IpSec_IOS(botClient, chatID, -1);

                        user.NameOS = NamesInlineButtons.IOS;
                        user.NameService = ipsecServer.NameCertainIPSec;
                        user.ServiceKey = chatID;
                        user.ServiceAddress = ipsecServer.ServerIPSec;
                    }
                    // ANDROID
                    else if (typeconnect == NamesInlineButtons.Mobile_TryFreePeriod_IpSec_android)
                    {
                        ipsecServer = await CreateAndSendConfig_IpSec_Android(botClient, chatID, -1);

                        user.NameOS = NamesInlineButtons.Android;
                        user.NameService = ipsecServer.NameCertainIPSec;
                        user.ServiceKey = chatID;
                        user.ServiceAddress = ipsecServer.ServerIPSec;
                    }
                    else if (typeconnect == NamesInlineButtons.Mobile_TryFreePeriod_Socks_ios)
                    {
                        resFuncSocksServer = await CreateAndSendKey_Socks(botClient, chatID, NamesInlineButtons.IOS, -1);

                        user.NameOS = NamesInlineButtons.IOS;
                        user.NameService = resFuncSocksServer.socksServer.NameCertainSocks;
                        user.ServiceKey = resFuncSocksServer.id_newUser;
                        user.ServiceAddress = resFuncSocksServer.socksServer.ServerSocks;
                    }
                    else if (typeconnect == NamesInlineButtons.Mobile_TryFreePeriod_Socks_android)
                    {
                        resFuncSocksServer = await CreateAndSendKey_Socks(botClient, chatID, NamesInlineButtons.Android, -1);

                        user.NameOS = NamesInlineButtons.Android;
                        user.NameService = resFuncSocksServer.socksServer.NameCertainSocks;
                        user.ServiceKey = resFuncSocksServer.id_newUser;
                        user.ServiceAddress = resFuncSocksServer.socksServer.ServerSocks;
                    }                  

                    await db.Users.AddAsync(user);
                    await db.SaveChangesAsync();
                    logger.LogInformation("Метод BotBeginFreePeriodAsync, FREE PERIOD новый пользователь добавлен, chatid: {chatid}", chatID);

                }
            }           
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

            var button1 = InlineKeyboardButton.WithCallbackData($"1 месяц за {TgBotHostedService.Price_1_Month} ₽.", $"{typepayment}_{NamesInlineButtons.Month_1}");
            var button2 = InlineKeyboardButton.WithCallbackData($"3 месяц за {TgBotHostedService.Price_3_Month} ₽.", $"{typepayment}_{NamesInlineButtons.Month_3}");
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
                    price = TgBotHostedService.Price_1_Month;
                    information_massage = $"Оплата подписки на 1 месяц сервиса {TgBotHostedService.BotName} для {device_info}";
                    break;
                case 3:
                    price = TgBotHostedService.Price_3_Month;
                    information_massage = $"Оплата подписки на 3 месяца сервиса {TgBotHostedService.BotName} для {device_info}";
                    break;
                default:
                    break;
            }

            var data = JsonConvert.SerializeObject(new { capture = true });
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
                providerData: data);

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
                            $"Ваш ID = " + info_chatid.ToString() + $". Подписка активна до {user.DateNextPayment.ToString("dd-MM-yyyy")} г.\n\n" +
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
        /// Метод создания пользователя и занесения в бд и формирования и выдачи конфига
        /// </summary>
        /// <param name="botClient"> бот клиент </param>
        /// <param name="payment"> обьект с информацией об оплате</param>
        /// <param name="update"> объект с информацией о пользователе и поступившей команде</param>
        /// <returns></returns>
        public async Task BotNewPaymentAsync(ITelegramBotClient botClient, SuccessfulPayment? payment, Telegram.Bot.Types.Update update)
        {
            using (TgVpnbotContext db = new TgVpnbotContext())
            {
                //db.Database.EnsureCreated();
                var chatID = update.Message.Chat.Id;

                var user = await db.Users.FirstOrDefaultAsync(u => u.ChatID == chatID);

                if (user != null) // если пользователь уже есть но он не активный
                {
                    user.Status = "active";
                    user.ProviderPaymentChargeId = payment?.ProviderPaymentChargeId;
                    user.TypeOfDevice = NamesInlineButtons.StartMobile;

                    logger.LogInformation("Метод BotNewPaymentAsync, успешная оплата, активировал старого пользователя, chatid: {chatid}, paymentID: {payID}, ОС: {os}",
                        chatID, payment?.ProviderPaymentChargeId, payment.InvoicePayload);
                }
                else // если новый пользователь
                {
                    user = new UserDB
                    {
                        FirstName = update.Message.Chat?.FirstName,
                        Username = update.Message.Chat?.Username,
                        ChatID = chatID,
                        ProviderPaymentChargeId = payment?.ProviderPaymentChargeId,
                        TypeOfDevice = NamesInlineButtons.StartMobile,
                    };

                    await db.Users.AddAsync(user);

                    logger.LogInformation("Метод BotNewPaymentAsync, успешная оплата, новый пользователь добавлен ДОЛЖЕН БЫЛ УЖЕ БЫТЬ, chatid: {chatid}, paymentID: {payID}, ОС: {os}",
                        chatID, payment?.ProviderPaymentChargeId, payment.InvoicePayload);
                }
               
                #region Идентификация сервера и ос оплаты, создание конфигов и ключей

                var match_ = Regex.Match(payment.InvoicePayload, @"Button_(?<service>.*)_(?<os>.*)_(?<period>\d+_.*)");
                if (match_.Success)
                {
                    // 1 MONTH
                    if (match_.Groups["period"].ToString() == NamesInlineButtons.Month_1)
                        user.DateNextPayment = DateTime.Now.AddMonths(1);
                    // 3 MONTH
                    else if (match_.Groups["period"].ToString() == NamesInlineButtons.Month_3)
                        user.DateNextPayment = DateTime.Now.AddMonths(3);

                    await botClient.SendTextMessageAsync(chatID, $"Оплата прошла успешно! ✅\n\n" +
                        $"Ваш ID = " + chatID.ToString() + $". Подписка активна до {user.DateNextPayment.ToString("dd-MM-yyyy")} г. включительно\n\n" +
                        $"Не удаляйте и не останавливайте бота, ближе к концу периода здесь появится новая ссылка оплаты, если вы решите остаться с нами. 🤗\n\n" +
                        $"*ID необходим для участия в реферальной программе и отслеживания периода оплаты. 😉");


                    //IPSEC
                    if (match_.Groups["service"].ToString() == NamesInlineButtons.IPSEC)
                    {
                        IIPsec1 ipsecServer = null;

                        // IOS
                        if (match_.Groups["os"].ToString() == NamesInlineButtons.IOS)
                        {
                            ipsecServer = await CreateAndSendConfig_IpSec_IOS(botClient, chatID, -1);
                            user.NameOS = NamesInlineButtons.IOS;                       
                        }
                        // ANDROID
                        else if(match_.Groups["os"].ToString() == NamesInlineButtons.Android)
                        {
                            ipsecServer = await CreateAndSendConfig_IpSec_Android(botClient, chatID, -1);
                            user.NameOS = NamesInlineButtons.Android;
                        }

                        user.NameService = ipsecServer.NameCertainIPSec;
                        user.ServiceKey = chatID;
                        user.ServiceAddress = ipsecServer.ServerIPSec;
                    }
                    // SOCKS
                    else if (match_.Groups["service"].ToString() == NamesInlineButtons.Socks)
                    {
                        (ISocks1 socksServer, long id_newUser) resFunc = (null, 0);

                        // IOS
                        if (match_.Groups["os"].ToString() == NamesInlineButtons.IOS)
                        {
                            resFunc = await CreateAndSendKey_Socks(botClient, chatID, NamesInlineButtons.IOS, -1);
                            user.NameOS = NamesInlineButtons.IOS;
                        }
                        // ANDROID
                        else if (match_.Groups["os"].ToString() == NamesInlineButtons.Android)
                        {
                            resFunc = await CreateAndSendKey_Socks(botClient, chatID, NamesInlineButtons.Android, -1);
                            user.NameOS = NamesInlineButtons.Android;
                        }

                        user.NameService = resFunc.socksServer.NameCertainSocks;
                        user.ServiceKey = resFunc.id_newUser;
                        user.ServiceAddress = resFunc.socksServer.ServerSocks;
                    }
                }
                
                #endregion

                await db.SaveChangesAsync();
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
                    if(ref_user.ProviderPaymentChargeId == NamesInlineButtons.Mobile_TryFreePeriod)
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
                            await botClient.SendTextMessageAsync(clientID, $"Пользователь с chatID: {chatid} проголосовал за вас, теперь вы его пригласитель  🥳");

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
                    user.Blatnoi = true;
                    await db.SaveChangesAsync();

                    await botClient.SendTextMessageAsync(1278048494, $"Сделал пользователя {chatID} блатным");
                    await botClient.SendTextMessageAsync(adminID, $"Сделал пользователя {chatID} блатным");
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

        ////////////////////////////////////////////////  СПОМОГАТЕЛЬНЫЕ МЕТОДЫ /////////////////////////////////////////////////////////////////////////////////////

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
        ///  Метод создания и отпрвки конфига IPSEC IOS 
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="chatID"> пользователь кому отправляется конфиг </param>
        /// <param name="ipsecServer"></param>
        /// <returns></returns>
        public async Task<IIPsec1> CreateAndSendConfig_IpSec_IOS(ITelegramBotClient botClient, long chatID, int numserver)
        {
            string ipsec_server_name = string.Empty;
            if (numserver > 0) // условие для команд админа
            {
                ipsec_server_name = TgBotHostedService.IPSEC_SERVERS_LIST[numserver - 1];
            }
            else
            {
                ipsec_server_name = await BotIPsecConfigBalanserAsync();
            }
            var ipsecServer = ipsecResolver(ipsec_server_name); // СОЗДАЮ КОНКРЕТНЫЙ СЕРВЕР ГДЕ МЕНЬШЕ 70 КОНФИГОВ

            await botClient.SendTextMessageAsync(chatID, IosInstructionIPSec);

            //await using Stream stream = System.IO.File.OpenRead(@"C:\Users\chin1\source\repos\TgBotNamelessNetwork\VideoInstructions\instruction_IOS_IPSEC_NEW.mp4");
            var mes = await botClient.SendVideoAsync(
            chatId: chatID,
            //video: "BAACAgIAAxkDAAIUUWYHJaIibWl1V3tdreo9O829c6EvAAKAQAACT21BSKKYYspolrvkNAQ"); // fileid video ios ipsec NamelessNetwork
        video: "BAACAgIAAxkBAAPXZJ9ombTpDVjZdPbUrIUnqI_H4KMAAjA0AAL0QfhIJJN1oofbubovBA"); // fileid video ios ipsec TestNamelessVPN
                                                                                          //video: new InputOnlineFile(content: stream, fileName: $"Тестовая инструкция"));

            await ipsecServer.CreateUserConfigAsync(botClient, chatID, "getconf", "mobileconfig", NamesInlineButtons.StartMobile); // Отправка данных на сервер и формирование конфига. Отсылка готового конфига пользователю

            return ipsecServer;
        }

        /// <summary>
        ///  Метод создания и отпрвки конфига IPSEC ANDROID 
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="chatID"> пользователь кому отправляется конфиг </param>
        /// <param name="ipsecServer"></param>
        /// <returns></returns>
        public async Task<IIPsec1> CreateAndSendConfig_IpSec_Android(ITelegramBotClient botClient, long chatID, int numserver)
        {
            string ipsec_server_name = string.Empty;
            if (numserver > 0)
            {
                ipsec_server_name = TgBotHostedService.IPSEC_SERVERS_LIST[numserver - 1];
            }
            else
            {
                ipsec_server_name = await BotIPsecConfigBalanserAsync();
            }
            var ipsecServer = ipsecResolver(ipsec_server_name); // СОЗДАЮ КОНКРЕТНЫЙ СЕРВЕР ГДЕ МЕНЬШЕ 100 КОНФИГОВ

            await botClient.SendTextMessageAsync(chatID, AndroidInstructionIPSec);
            var match_android = Regex.Match(ipsecServer.ServerIPSec, @"https:\/\/(?<ip>.*):8433");
            if (match_android.Success)
            {
                await botClient.SendTextMessageAsync(chatID, "Ваш сервер (пункт 6 иструкции): " + match_android.Groups["ip"].ToString());
            }

            //await using Stream stream = System.IO.File.OpenRead(@"C:\Users\chin1\source\repos\TelegramBotProject\instruction_Android_IPSec.mp4");
            var mes = await botClient.SendVideoAsync(
            chatId: chatID,
            video: "BAACAgIAAxkDAANmZWekmIC3iGr0G-cTk9eovK1ts_UAAuU0AAIlBUFLUNBOX1gAARJaMwQ"); // fileid video NamelessNetwork
                                                                                               //video: "BAACAgIAAxkBAAOsZJ9CPCG7Nrfw9Ip3iJ69Z4Dxk0kAAo8tAAIh_wFJw_dGYKpZ5gkvBA"); // fileid video TestNamelessVPN
                                                                                               //video: new InputOnlineFile(content: stream, fileName: $"Тестовая инструкция"));

            await ipsecServer.CreateUserConfigAsync(botClient, chatID, "getconfandroid", "p12", NamesInlineButtons.StartMobile); // Отправка данных на сервер и формирование конфига. Отсылка готового конфига пользователю

            return ipsecServer;
        }

        /// <summary>
        /// Метод создания и отпрвки ключа SOCKS 
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="chatID"></param>
        /// <param name="socksServer"></param>
        /// <param name="os"> Выбранная операционная система </param>
        /// <returns></returns>
        public async Task<(ISocks1 socksServer, long id_newUser)> CreateAndSendKey_Socks(ITelegramBotClient botClient, long chatID, string os, int numserver)
        {
            string socks_server_name = string.Empty;
            if (numserver > 0)
            {
                socks_server_name = TgBotHostedService.SOCKS_SERVERS_LIST[numserver - 1];
            }
            else
            {
                socks_server_name = await BotSocksKeyBalanserAsync();
            }
            var socksServer = socksResolver(socks_server_name);

            if (os == NamesInlineButtons.IOS)
            {
                await botClient.SendTextMessageAsync(chatID, IosInstructionSocks);

                //await using Stream stream = System.IO.File.OpenRead(@"C:\Users\chin1\source\repos\TelegramBotProject\instruction_IOS_Socks.mp4");
                var mes = await botClient.SendVideoAsync(
                chatId: chatID,
                //video: "BAACAgIAAxkDAANyZWiG0AABrL2md4eb79UH7ZcR9hAwAAIVOgACJQVBS_cx0M0jD2nHMwQ"); // fileid video NamelessNetwork
                                                            video: "BAACAgIAAxkDAAIEuWVk4XVhHJietr3md_CsBlqB7qkfAAKSRgACFFsoS-6Q2XM11qLpMwQ"); // fileid video TestNamelessVPN
                                          //video: new InputOnlineFile(content: stream, fileName: $"Тестовая инструкция"));
            }
            else // android
            {
                await botClient.SendTextMessageAsync(chatID, AndroidInstructionSocks);

                //await using Stream stream = System.IO.File.OpenRead(@"C:\Users\chin1\source\repos\TelegramBotProject\instruction_Android_Socks.mp4");
                var mes = await botClient.SendVideoAsync(
                chatId: chatID,
                video: "BAACAgIAAxkDAAP-ZWtTHDPYgBL6VNBgziSD_Hub36wAAms7AAItb1hLpnS9NuMaoDwzBA"); // fileid video NamelessNetwork
            //video: "BAACAgIAAxkDAAIEy2Vk5AXzdL2PjAJ9sdK4L4FLRmwgAAKxRgACFFsoS8UG2N-cZVIwMwQ"); // fileid video TestNamelessVPN
                                //video: new InputOnlineFile(content: stream, fileName: $"Тестовая инструкция"));
            }

            var id_newUser = await socksServer.CreateUserAsync(botClient, chatID);

            return (socksServer: socksServer, id_newUser: id_newUser);

        }

        /// <summary>
        /// Вспомогательный метод для получения всех ID пользователей из бд и отправке им сообщения
        /// </summary>
        /// <param name="typeService"></param>
        /// <returns></returns>
        public async Task<int> BotSendMessageToUsersInDBAsync(ITelegramBotClient botClient, string typeService, string text)
        {
            List<long> listID = new List<long>();
            int count = 0;
            using (TgVpnbotContext db = new TgVpnbotContext())
            {
                if (typeService == NamesInlineButtons.IPSEC_android)
                {
                    listID = await db.Users
                    .Where(u => u.Status == "active" && u.Blatnoi == false && u.NameOS == NamesInlineButtons.Android && EF.Functions.Like(u.NameService, $"{NamesInlineButtons.IPSEC}%"))
                    .Select(user => user.ChatID).ToListAsync().ConfigureAwait(false);
                }
                if (typeService == NamesInlineButtons.IPSEC_ios)
                {
                    listID = await db.Users
                    .Where(u => u.Status == "active" && u.Blatnoi == false && u.NameOS == NamesInlineButtons.IOS && EF.Functions.Like(u.NameService, $"{NamesInlineButtons.IPSEC}%"))
                    .Select(user => user.ChatID).ToListAsync().ConfigureAwait(false);
                }
                if (typeService == NamesInlineButtons.Socks)
                {
                    listID = await db.Users
                    .Where(u => u.Status == "active" && u.Blatnoi == false && EF.Functions.Like(u.NameService, $"{NamesInlineButtons.Socks}%"))
                    .Select(user => user.ChatID).ToListAsync().ConfigureAwait(false);
                }
                if (typeService == NamesInlineButtons.AllUsers)
                {
                    listID = await db.Users
                    .Where(u => u.Status == "active" && u.Blatnoi == false)
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
                catch (Exception ex) {
                    testBlock.Add(uID);
                    logger.LogInformation("match_send_messahe_to_IPSecUsers {ex}", ex); 
                }
            }

            return count;

        }

    }
}
