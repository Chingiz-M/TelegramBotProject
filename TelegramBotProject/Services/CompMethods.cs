using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types.InputFiles;
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
                        $"84722 — это надежный и высокоскоростной VPN сервис с серверами в Нидерландах 🇳🇱, Израиле 🇮🇱 и Молдове 🇲🇩.\n\n" +
                        $"Мы предлагаем:\n" +
                        $"🚀 Высокую скорость\n" +
                        $"🌗 Выбор из двух наиболее актуальных типов подключения\n" +
                        $"💳 Возможность оплаты картами РФ\n" +
                        $"💰 Бесплатный пробный период в 2 недели и самую низкую цену на рынке!\n\n" +
                        $"Стоимость подписки после пробного периода составит 99 ₽/мес.\n\n" +
                        $"Действует реферальная система: пригласите 3 друзей в наш сервис и получите 3 месяца бесплатно." +
                        $"Комп";
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
                if (count_users < TgBotHostedService.Comp_CountINServerIpSec)
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
                    $"Ты использовал свой пробный период. Мы надеемся, что не разочаровали. 🙌\n" +
                    $"Чтобы продолжить пользоваться сервисом, необходимо приобрести подписку.\n\n" +
                    $"Стоимость подписки после пробного периода составит 99 ₽/ мес.";
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
        public async Task Comp_BotBeginFreePeriodAsync(ITelegramBotClient botClient, Telegram.Bot.Types.CallbackQuery button, string typeconnect)
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

                    await botClient.SendTextMessageAsync(chatID, $"Бесплатный период начался! ✅\n\n" +
                        $"Ваш ID = " + CompChatID.ToString() + $". Подписка активна до {user.DateNextPayment.ToString("dd-MM-yyyy")} г. включительно\n\n" +
                        $"Не удаляйте и не останавливайте бота, ближе к концу периода здесь появится ссылка оплаты, если вы решите остаться с нами. 🤗\n\n" +
                        $"*ID необходим для участия в реферальной программе и отслеживания периода оплаты. 😉");


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

            //await using Stream stream = System.IO.File.OpenRead(@"C:\Users\chin1\source\repos\TgBotNamelessNetwork\VideoInstructions\instruction_IOS_IPSEC_NEW.mp4");
            var mes = await botClient.SendVideoAsync(
            chatId: chatID,
                //video: "BAACAgIAAxkDAAIUUWYHJaIibWl1V3tdreo9O829c6EvAAKAQAACT21BSKKYYspolrvkNAQ"); // fileid video ios ipsec NamelessNetwork
                video: "BAACAgIAAxkBAAPXZJ9ombTpDVjZdPbUrIUnqI_H4KMAAjA0AAL0QfhIJJN1oofbubovBA"); // fileid video ios ipsec TestNamelessVPN
                                                                                                  //video: new InputOnlineFile(content: stream, fileName: $"Тестовая инструкция"));

            await comp_ipsecServer.CreateUserConfigAsync(botClient, chatID, "getconf", "mobileconfig"); // Отправка данных на сервер и формирование конфига. Отсылка готового конфига пользователю

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

            await botClient.SendTextMessageAsync(chatID, AndroidInstructionIPSec);
            var match_android = Regex.Match(comp_ipsecServer.ServerIPSec, @"https:\/\/(?<ip>.*):8433");
            if (match_android.Success)
            {
                await botClient.SendTextMessageAsync(chatID, "Ваш сервер (пункт 6 иструкции): " + match_android.Groups["ip"].ToString());
            }

            //await using Stream stream = System.IO.File.OpenRead(@"C:\Users\chin1\source\repos\TelegramBotProject\instruction_Android_IPSec.mp4");
            var mes = await botClient.SendVideoAsync(
            chatId: chatID,
            //video: "BAACAgIAAxkDAANmZWekmIC3iGr0G-cTk9eovK1ts_UAAuU0AAIlBUFLUNBOX1gAARJaMwQ"); // fileid video NamelessNetwork
                                            video: "BAACAgIAAxkBAAOsZJ9CPCG7Nrfw9Ip3iJ69Z4Dxk0kAAo8tAAIh_wFJw_dGYKpZ5gkvBA"); // fileid video TestNamelessVPN
                     //video: new InputOnlineFile(content: stream, fileName: $"Тестовая инструкция"));

            await comp_ipsecServer.CreateUserConfigAsync(botClient, chatID, "getconfandroid", "p12"); // Отправка данных на сервер и формирование конфига. Отсылка готового конфига пользователю

            return comp_ipsecServer;
        }

    }
}
