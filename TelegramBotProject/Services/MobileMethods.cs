using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types.Payments;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotProject.DB;
using TelegramBotProject.Entities;
using TelegramBotProject.Intarfaces;
using TelegramBotProject.TelegramBot;
using static TelegramBotProject.StartUp;

namespace TelegramBotProject.Services
{
    internal class MobileMethods : IMobileMethods
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
        private readonly IBotCommands botcommands;
        private readonly IPSecServiceResolver ipsecResolver;
        private readonly SOCKSServiceResolver socksResolver;

        public MobileMethods(ILogger<MobileMethods> logger, IPSecServiceResolver ipsecResolver, SOCKSServiceResolver socksResolver, IBotCommands botcommands)
        {
            this.logger = logger;
            this.ipsecResolver = ipsecResolver;
            this.socksResolver = socksResolver;
            this.botcommands = botcommands;
        }

        /// <summary>
        /// Балансер конфигов IPsec, ищет где меньше 100 конфигов и отпрляет имя сервака
        /// </summary>
        /// <returns></returns>
        public async Task<string> Mobile_BotIPsecConfigBalanserAsync()
        {
            foreach (var item in TgBotHostedService.IPSEC_SERVERS_LIST)
            {
                if (item != "IPSEC_1")
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
        public async Task<string> Mobile_BotSocksKeyBalanserAsync()
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="chatid"></param>
        /// <returns></returns>
        public async Task Mobile_BotStartAsync(ITelegramBotClient botClient, long chatid)
        {
            var user_mobile = await botcommands.BotCheckUserBDAsync(chatid, 2); // есть ли ваще

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

                await Mobile_SelectServiceAsync(botClient, chatid, TgBotHostedService.TypeConnect.Payment);
            }

        }

        /// <summary>
        /// Метод выбор сервиса и начало пробного либо платного периодов
        /// </summary>
        /// <param name="botClient"> бот клиент </param>
        /// <param name="chatid"> чат айди пользователя который голосует </param>
        /// <param name="connect"> тип подключения бесплтаный или платный </param>
        /// <returns></returns>
        public async Task Mobile_SelectServiceAsync(ITelegramBotClient botClient, long chatid, TgBotHostedService.TypeConnect connect)
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
        public async Task Mobile_SelectOpSysAsync(ITelegramBotClient botClient, long chatid, string? FirstName, string ServiceName)
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
        public async Task Mobile_BeginFreePeriodAsync(ITelegramBotClient botClient, Telegram.Bot.Types.CallbackQuery button, string typeconnect)
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
                        ipsecServer = await Mobile_CreateAndSendConfig_IpSec_IOS(botClient, chatID, -1);

                        user.NameOS = NamesInlineButtons.IOS;
                        user.NameService = ipsecServer.NameCertainIPSec;
                        user.ServiceKey = chatID;
                        user.ServiceAddress = ipsecServer.ServerIPSec;
                    }
                    // ANDROID
                    else if (typeconnect == NamesInlineButtons.Mobile_TryFreePeriod_IpSec_android)
                    {
                        ipsecServer = await Mobile_CreateAndSendConfig_IpSec_Android(botClient, chatID, -1);

                        user.NameOS = NamesInlineButtons.Android;
                        user.NameService = ipsecServer.NameCertainIPSec;
                        user.ServiceKey = chatID;
                        user.ServiceAddress = ipsecServer.ServerIPSec;
                    }
                    else if (typeconnect == NamesInlineButtons.Mobile_TryFreePeriod_Socks_ios)
                    {
                        resFuncSocksServer = await Mobile_CreateAndSendKey_Socks(botClient, chatID, NamesInlineButtons.IOS, -1);

                        user.NameOS = NamesInlineButtons.IOS;
                        user.NameService = resFuncSocksServer.socksServer.NameCertainSocks;
                        user.ServiceKey = resFuncSocksServer.id_newUser;
                        user.ServiceAddress = resFuncSocksServer.socksServer.ServerSocks;
                    }
                    else if (typeconnect == NamesInlineButtons.Mobile_TryFreePeriod_Socks_android)
                    {
                        resFuncSocksServer = await Mobile_CreateAndSendKey_Socks(botClient, chatID, NamesInlineButtons.Android, -1);

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
        /// Метод создания пользователя и занесения в бд и формирования и выдачи конфига
        /// </summary>
        /// <param name="botClient"> бот клиент </param>
        /// <param name="payment"> обьект с информацией об оплате</param>
        /// <param name="update"> объект с информацией о пользователе и поступившей команде</param>
        /// <returns></returns>
        public async Task Mobile_NewPaymentAsync(ITelegramBotClient botClient, SuccessfulPayment? payment, Telegram.Bot.Types.Update update)
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
                            ipsecServer = await Mobile_CreateAndSendConfig_IpSec_IOS(botClient, chatID, -1);
                            user.NameOS = NamesInlineButtons.IOS;
                        }
                        // ANDROID
                        else if (match_.Groups["os"].ToString() == NamesInlineButtons.Android)
                        {
                            ipsecServer = await Mobile_CreateAndSendConfig_IpSec_Android(botClient, chatID, -1);
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
                            resFunc = await Mobile_CreateAndSendKey_Socks(botClient, chatID, NamesInlineButtons.IOS, -1);
                            user.NameOS = NamesInlineButtons.IOS;
                        }
                        // ANDROID
                        else if (match_.Groups["os"].ToString() == NamesInlineButtons.Android)
                        {
                            resFunc = await Mobile_CreateAndSendKey_Socks(botClient, chatID, NamesInlineButtons.Android, -1);
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


        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        ///  Метод создания и отпрвки конфига IPSEC IOS 
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="chatID"> пользователь кому отправляется конфиг </param>
        /// <param name="ipsecServer"></param>
        /// <returns></returns>
        public async Task<IIPsec1> Mobile_CreateAndSendConfig_IpSec_IOS(ITelegramBotClient botClient, long chatID, int numserver)
        {
            string ipsec_server_name = string.Empty;
            if (numserver > 0) // условие для команд админа
            {
                ipsec_server_name = TgBotHostedService.IPSEC_SERVERS_LIST[numserver - 1];
            }
            else
            {
                ipsec_server_name = await Mobile_BotIPsecConfigBalanserAsync();
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
        public async Task<IIPsec1> Mobile_CreateAndSendConfig_IpSec_Android(ITelegramBotClient botClient, long chatID, int numserver)
        {
            string ipsec_server_name = string.Empty;
            if (numserver > 0)
            {
                ipsec_server_name = TgBotHostedService.IPSEC_SERVERS_LIST[numserver - 1];
            }
            else
            {
                ipsec_server_name = await Mobile_BotIPsecConfigBalanserAsync();
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
        public async Task<(ISocks1 socksServer, long id_newUser)> Mobile_CreateAndSendKey_Socks(ITelegramBotClient botClient, long chatID, string os, int numserver)
        {
            string socks_server_name = string.Empty;
            if (numserver > 0)
            {
                socks_server_name = TgBotHostedService.SOCKS_SERVERS_LIST[numserver - 1];
            }
            else
            {
                socks_server_name = await Mobile_BotSocksKeyBalanserAsync();
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
    }
}
