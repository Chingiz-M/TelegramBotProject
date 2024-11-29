using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.Payments;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotProject.DB;
using TelegramBotProject.Entities;
using TelegramBotProject.Intarfaces;
using TelegramBotProject.TelegramBot;
using static TelegramBotProject.StartUp;

namespace TelegramBotProject.Services
{
    internal class CompMethods : ICompMethods
    {
        #region Instructions

        private string IosInstructionIPSec = "Инструкция для MacOS\n\n" +
                        "1. Скачайте файл *.mobileconfig на ваше MacOS устройство.\n" +
                        "2. Дважды щелкните по файлу, появится уведомление об установке профиля.\n" +
                        "3. Перейдите в Системные настройки > Профили.\n" +
                        "4. В окне \"Профили\" выберите ваш файл и нажмите \"Установить\".\n" +
                        "5. При появлении запроса о подтверждении снова нажмите \"Установить\".\n" +
                        "6. В меню Apple перейдите в Системные настройки > Сеть.\n" +
                        "7. Включите установленный VPN-профиль.";

        private string WindowsInstruction = "Инструкция для Windows 10/11:\n\n" +
                        "1. Скачайте ваш файл *.p12 и установочный cmd-скрипт на ваш ПК, поместив их вместе в отдельную папку (проследите, чтобы путь к папке не содержал кириллических наименований).\n" +
                        "2. Разблокируйте cmd-скрипт, нажав на него правой кнопкой мыши и далее: Свойства > Поставить галочку на \"Разблокировать\" > Применить > Оk.\n" +
                        "3. Запустить cmd-скрипт от имени администратора (через правый щелчок мыши).\n" +
                        "4. В черном окне терминала первым запросом будет проверка имени загружаемого файла \"VPN client name:\", просто нажмите Enter.\n" +
                        "5. Далее по запросу \"VPN server address:\" введите ip-адрес вашего сервера (набор из 4-х чисел, разделенных точками), который прислал наш бот.\n" +
                        "6. При запросе \"IKEv2 connection name:\" нажмите Enter.\n" +
                        "7. Установленный VPN отобразится в полосе Пуск внутри иконки \"Сеть\", в правом нижнем углу экрана.\n" +
                        "8. Подключите VPN, нажав на соответствующую кнопку.";

        private string StartText = $"Вы выбрали опцию для ПК (Windows 10/11 и MacOS) и YouTube 🖥️.";
        #endregion

        private ILogger logger;
        private readonly Comp_IPSecServiceResolver comp_ipsecResolver;
        private readonly IBotCommands botcommands;


        public CompMethods(ILogger<CompMethods> logger, Comp_IPSecServiceResolver ipsecResolver, IBotCommands botcommands)
        {
            this.logger = logger;
            this.comp_ipsecResolver = ipsecResolver;
            this.botcommands = botcommands;
        }

        /// <summary>
        /// Балансер конфигов IPsec, ищет где меньше заданного числа конфигов и отпрляет имя сервака
        /// </summary>
        /// <returns></returns>
        public async Task<string> Comp_BotIPsecConfigBalanserAsync()
        {
            foreach (var item in TgBotHostedService.Comp_IPSEC_SERVERS_LIST)
            {
                var temp_server = comp_ipsecResolver(item);
                var count_users = await temp_server.GetTotalUserAsync();
                if (count_users < TgBotHostedService.Comp_CountINServer)
                    return item;
            }

            return TgBotHostedService.Comp_IPSEC_SERVERS_LIST[0];
        }

        public async Task Comp_BotStartAsync(ITelegramBotClient botClient, long chatid)
        {
            var user_comp = await botcommands.BotCheckUserBDAsync(chatid * TgBotHostedService.USERS_COMP, 2); // есть ли ваще id в бд для компов

            if (user_comp == null) // если нажал старт новый пользователь которого нет в бд 
            {
                var button = InlineKeyboardButton.WithCallbackData("🔥 Попробовать бесплатный период 🔥", NamesInlineButtons.Comp_TryFreePeriod);
                var row = new InlineKeyboardButton[] { button };
                var keyboard = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup(row);

                await botClient.SendTextMessageAsync(chatid, StartText, replyMarkup: keyboard);
            }
            else // пользователь нажал старт и он есть в бд
            {
                string startText = $"Привет тебе, Путник!👋 Рады видеть тебя снова!\n\n" +
                    $"Ты использовал свой пробный период для ПК. Мы надеемся, что не разочаровали. 🙌\n" +
                    $"Чтобы продолжить пользоваться сервисом, необходимо приобрести подписку.\n\n" +
                    $"Стоимость подписки после пробного периода составит от 99 ₽/ мес.";
                await botClient.SendTextMessageAsync(chatid, startText);

                await Comp_SelectOpSysAsync(botClient, chatid, NamesInlineButtons.Comp_Payment);
            }
        }

        /// <summary>
        /// Метод выбора ОС на комп (бесплатный и платный период)
        /// </summary>
        /// <param name="botClient"> бот клиент </param>
        /// <param name="chatid"> чат айди клиента </param>
        /// <param name="ServiceName"> имя сервиса что бы добавить в payload для идентификации inline кнопки</param>
        /// <returns></returns>
        public async Task Comp_SelectOpSysAsync(ITelegramBotClient botClient, long chatid, string ServiceName)
        {
            var button1 = InlineKeyboardButton.WithCallbackData("Windows 🪟", $"{ServiceName}_{NamesInlineButtons.Windows}");
            var button2 = InlineKeyboardButton.WithCallbackData("MacOS 🍏", $"{ServiceName}_{NamesInlineButtons.MacOS}");
            var row1 = new InlineKeyboardButton[] { button1, button2 };
            var keyboard = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup(row1);

            await botClient.SendTextMessageAsync(chatid, $"Теперь выбери свою операционную систему. 👀", replyMarkup: keyboard);

        }

        /// <summary>
        /// Метод начала пробного периода
        /// Создание конфига и занесение пользователя в БД 
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="button"></param>
        /// <param name="typeconnect"></param>
        /// <returns></returns>
        public async Task Comp_BeginFreePeriodAsync(ITelegramBotClient botClient, Telegram.Bot.Types.CallbackQuery button, string typeconnect)
        {
            using (TgVpnbotContext db = new TgVpnbotContext())
            {
                var chatID = button.Message.Chat.Id;
                var CompChatID = chatID * TgBotHostedService.USERS_COMP;
                var user = await db.Users.FirstOrDefaultAsync(u => u.ChatID == CompChatID).ConfigureAwait(false);

                if (user != null) // если пользователь уже есть но его быть не должно
                {
                    await botClient.SendTextMessageAsync(1278048494, $"ERROR Метод BotBeginFreePeriodAsync пользователь id {CompChatID} есть в бд но не должно быть его"); // присылаю себе

                    logger.LogInformation("Метод BotBeginFreePeriodAsync пользователь id {chatID} есть в бд но не должно быть его", CompChatID);
                }
                else // если новый пользователь
                {
                    user = new UserDB
                    {
                        FirstName = button.Message.Chat?.FirstName,
                        Username = button.Message.Chat?.Username,
                        ChatID = CompChatID,
                        ProviderPaymentChargeId = NamesInlineButtons.Comp_TryFreePeriod,
                        TypeOfDevice = NamesInlineButtons.StartComp,
                    };

                    await botClient.SendTextMessageAsync(chatID, $"Бесплатный период для ПК начался! ✅\n\n" +
                        $"Ваш ID = " + chatID.ToString() + $". Подписка активна до {user.DateNextPayment.ToString("dd-MM-yyyy")} г. включительно\n\n" +
                        $"Не удаляйте и не останавливайте бота, ближе к концу периода здесь появится ссылка оплаты, если вы решите остаться с нами. 🤗\n\n" +
                        $"*ID необходим для отслеживания периода оплаты. 😉");


                    IIPsec1 comp_ipsecServer = null;

                    // MacOS
                    if (typeconnect == NamesInlineButtons.Comp_TryFreePeriod_MacOS)
                    {
                        comp_ipsecServer = await Comp_CreateAndSendConfig_IpSec_MacOS(botClient, chatID, -1);

                        user.NameOS = NamesInlineButtons.MacOS;
                        user.NameService = comp_ipsecServer.NameCertainIPSec;
                        user.ServiceKey = CompChatID;
                        user.ServiceAddress = comp_ipsecServer.ServerIPSec;
                    }
                    // Windows
                    else if (typeconnect == NamesInlineButtons.Comp_TryFreePeriod_Windows)
                    {
                        comp_ipsecServer = await Comp_CreateAndSendConfig_IpSec_Windows(botClient, chatID, -1);

                        user.NameOS = NamesInlineButtons.Windows;
                        user.NameService = comp_ipsecServer.NameCertainIPSec;
                        user.ServiceKey = CompChatID;
                        user.ServiceAddress = comp_ipsecServer.ServerIPSec;
                    }


                    await db.Users.AddAsync(user);
                    await db.SaveChangesAsync();
                    logger.LogInformation("Метод Comp_BotBeginFreePeriodAsync, FREE PERIOD для компа новый пользователь добавлен, chatid: {chatid}", chatID);

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
        public async Task Comp_NewPaymentAsync(ITelegramBotClient botClient, SuccessfulPayment? payment, Telegram.Bot.Types.Update update)
        {
            using (TgVpnbotContext db = new TgVpnbotContext())
            {
                //db.Database.EnsureCreated();
                var chatID = update.Message.Chat.Id;
                var CompChatID = chatID * TgBotHostedService.USERS_COMP;

                var user = await db.Users.FirstOrDefaultAsync(u => u.ChatID == CompChatID);

                if (user != null) // если пользователь уже есть но он не активный
                {
                    user.Status = "active";
                    user.ProviderPaymentChargeId = payment?.ProviderPaymentChargeId;
                    user.TypeOfDevice = NamesInlineButtons.StartComp;

                    logger.LogInformation("Метод Comp_NewPaymentAsync, успешная оплата, активировал старого пользователя, chatid: {chatid}, paymentID: {payID}, ОС: {os}",
                        CompChatID, payment?.ProviderPaymentChargeId, payment.InvoicePayload);
                }
                else // если новый пользователь код не нужен скорее всего но пока пусть будет на всякий
                {
                    user = new UserDB
                    {
                        FirstName = update.Message.Chat?.FirstName,
                        Username = update.Message.Chat?.Username,
                        ChatID = CompChatID,
                        ProviderPaymentChargeId = payment?.ProviderPaymentChargeId,
                        TypeOfDevice = NamesInlineButtons.StartComp,
                    };

                    await db.Users.AddAsync(user);

                    logger.LogInformation("Метод Comp_NewPaymentAsync, успешная оплата, новый пользователь добавлен ДОЛЖЕН БЫЛ УЖЕ БЫТЬ, chatid: {chatid}, paymentID: {payID}, ОС: {os}",
                        CompChatID, payment?.ProviderPaymentChargeId, payment.InvoicePayload);
                }

                #region Идентификация сервера и ос оплаты, создание конфигов и ключей

                var match_ = Regex.Match(payment.InvoicePayload, @"(?<service>.*)_(?<os>.*)_(?<period>\d+_.*)");
                if (match_.Success)
                {
                    // 1 MONTH
                    if (match_.Groups["period"].ToString() == NamesInlineButtons.Month_1)
                        user.DateNextPayment = DateTime.Now.AddMonths(1);
                    // 3 MONTH
                    else if (match_.Groups["period"].ToString() == NamesInlineButtons.Month_3)
                        user.DateNextPayment = DateTime.Now.AddMonths(3);

                    await botClient.SendTextMessageAsync(chatID, $"Оплата прошла успешно! ✅ (ПК)\n\n" +
                        $"Ваш ID = " + chatID.ToString() + $". Подписка активна до {user.DateNextPayment.ToString("dd-MM-yyyy")} г. включительно\n\n" +
                        $"Не удаляйте и не останавливайте бота, ближе к концу периода здесь появится новая ссылка оплаты, если вы решите остаться с нами. 🤗\n\n" +
                        $"*ID необходим для отслеживания периода оплаты. 😉");


                    IIPsec1 comp_ipsecServer = null;

                    // MacOS
                    if (match_.Groups["os"].ToString() == NamesInlineButtons.MacOS)
                    {
                        comp_ipsecServer = await Comp_CreateAndSendConfig_IpSec_MacOS(botClient, chatID, -1);
                        user.NameOS = NamesInlineButtons.MacOS;
                    }

                    // Windows
                    else if (match_.Groups["os"].ToString() == NamesInlineButtons.Windows)
                    {
                        comp_ipsecServer = await Comp_CreateAndSendConfig_IpSec_Windows(botClient, chatID, -1);
                        user.NameOS = NamesInlineButtons.Windows;
                    }

                    user.NameService = comp_ipsecServer.NameCertainIPSec;
                    user.ServiceKey = CompChatID;
                    user.ServiceAddress = comp_ipsecServer.ServerIPSec;

                }

                #endregion

                await db.SaveChangesAsync();
            }
        }

        /// <summary>
        ///  Метод создания и отпрвки конфига IPSEC MacOS 
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="chatID"> пользователь кому отправляется конфиг </param>
        /// <param name="numserver"> переменная для определения конкретного сервака при админской команде</param>
        /// <returns></returns>
        public async Task<IIPsec1> Comp_CreateAndSendConfig_IpSec_MacOS(ITelegramBotClient botClient, long chatID, int numserver)
        {
            string ipsec_server_name = string.Empty;
            if (numserver > 0) // условие для команд админа
            {
                ipsec_server_name = TgBotHostedService.Comp_IPSEC_SERVERS_LIST[numserver - 1];
            }
            else
            {
                ipsec_server_name = await Comp_BotIPsecConfigBalanserAsync();
            }
            var comp_ipsecServer = comp_ipsecResolver(ipsec_server_name); // СОЗДАЮ КОНКРЕТНЫЙ СЕРВЕР ГДЕ МЕНЬШЕ 70 КОНФИГОВ

            await botClient.SendTextMessageAsync(chatID, IosInstructionIPSec);

            //await using Stream stream = System.IO.File.OpenRead(@"C:\Users\chin1\source\repos\TgBotNamelessNetwork\VideoInstructions\instruction_MacOS.MOV");
            var mes = await botClient.SendVideoAsync(
            chatId: chatID,
            video: "BAACAgIAAxkDAAJh0Wb5vuXM7NecB3HBZjbfHxSAwVX_AAJ4XgAC04TRS2bPS3BwVngONgQ"); // fileid video  NamelessNetwork
            //video: "BAACAgIAAxkDAAIKAWb5o558xZvmzXEoC7B7zNRDBeWpAAJWYgACkZLIS9q_x-z02JaPNgQ"); // fileid video TestNamelessVPN
            //video: new InputOnlineFile(content: stream, fileName: $"Тестовая инструкция"));

            await comp_ipsecServer.CreateUserConfigAsync(botClient, chatID, "getconf", "mobileconfig", NamesInlineButtons.StartComp); // Отправка данных на сервер и формирование конфига. Отсылка готового конфига пользователю

            return comp_ipsecServer;
        }


        /// <summary>
        ///  Метод создания и отпрвки конфига IPSEC Windows 
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="chatID"> пользователь кому отправляется конфиг </param>
        /// <param name="numserver"> переменная для определения конкретного сервака при админской команде</param>
        /// <returns></returns>
        public async Task<IIPsec1> Comp_CreateAndSendConfig_IpSec_Windows(ITelegramBotClient botClient, long chatID, int numserver)
        {
            string ipsec_server_name = string.Empty;
            if (numserver > 0)
            {
                ipsec_server_name = TgBotHostedService.Comp_IPSEC_SERVERS_LIST[numserver - 1];
            }
            else
            {
                ipsec_server_name = await Comp_BotIPsecConfigBalanserAsync();
            }
            var comp_ipsecServer = comp_ipsecResolver(ipsec_server_name); // СОЗДАЮ КОНКРЕТНЫЙ СЕРВЕР ГДЕ МЕНЬШЕ ЗАДАННОГО ЧИСЛА ПОЛЬЗОВАТЕЛЕЙ

            await botClient.SendTextMessageAsync(chatID, WindowsInstruction);
            var match_android = Regex.Match(comp_ipsecServer.ServerIPSec, @"https:\/\/(?<ip>.*):8433");
            if (match_android.Success)
            {
                await botClient.SendTextMessageAsync(chatID, "Ваш сервер (пункт 4 иструкции): " + match_android.Groups["ip"].ToString());
            }

            //await using Stream stream = System.IO.File.OpenRead(@"C:\Users\chin1\source\repos\TgBotNamelessNetwork\VideoInstructions\instruction_Windows.mp4");
            var mes = await botClient.SendVideoAsync(
            chatId: chatID,
            video: "BAACAgIAAxkDAAJhx2b5vpGslhzoKzmPoNWNRnB_BgS1AAJzXgAC04TRS4cv1mJ9i_udNgQ"); // fileid video NamelessNetwork
            //video: "BAACAgIAAxkDAAII82bzHTZUOvZ7y0LiuCGpDP-Lg9ytAAIKYAACcWGYS99SjiH5Q4v8NgQ"); // fileid video TestNamelessVPN
                                                                                               //video: new InputOnlineFile(content: stream, fileName: $"Тестовая инструкция"));

            //using (Stream streamf = System.IO.File.OpenRead(@"C:\Users\chin1\source\repos\TgBotNamelessNetwork\VideoInstructions\ikev2_config_import_Win.cmd"))
            //{
            var message = await botClient.SendDocumentAsync(
                    chatId: chatID,
                    //document: new InputOnlineFile(content: streamf, fileName: "ikev2_config_import_Win.cmd"));
                    //document: new InputOnlineFile("BQACAgIAAxkDAAIJI2bzI_jI02uwhYvbOlgwCL4qDsPZAAJ5YAACcWGYS4EXuMlc68DbNgQ")); // TestNamelessVPN cmd fileid
                    document: new InputOnlineFile("BQACAgIAAxkDAAJhyGb5vpGknyfNPq53riVZuhXytw26AAJ0XgAC04TRSyyKgJB5wyv3NgQ")); // NamelessNetwork cmd fileid
                                                                                                                   //}

            await comp_ipsecServer.CreateUserConfigAsync(botClient, chatID, "getconfandroid", "p12", NamesInlineButtons.StartComp); // Отправка данных на сервер и формирование конфига. Отсылка готового конфига пользователю

            return comp_ipsecServer;
        }
    }
}
