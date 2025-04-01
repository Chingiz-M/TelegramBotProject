using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotProject.DB;
using TelegramBotProject.Entities;
using TelegramBotProject.Intarfaces;
using static TelegramBotProject.StartUp;

namespace TelegramBotProject.TelegramBot
{
    internal class TgBotHostedService : BackgroundService
    {
        /// <summary>
        /// Токен телеграм бота для работы, в файле appsettings.json
        /// </summary>
        static private string Token { get; } = StartUp.GetTokenfromConfig("Token");
        /// <summary>
        /// Токен платежной системы для оплаты подписки, в файле appsettings.json
        /// </summary>
        static public string TokenPayment { get; } = StartUp.GetTokenfromConfig("API_KEY_YOUKASSA");
        /// <summary>
        /// Объект телеграмм бота для работы
        /// </summary>
        static public ITelegramBotClient bot = new TelegramBotClient(Token);
        /// <summary>
        /// Переменная заглушка во время починки
        /// </summary>
        static private bool BOT_FIX_MODE { get; set; } = false;
        static public string BotName { get; } = StartUp.GetTokenfromConfig("BotName");
        static public int Price_1_Month_mobile { get; } = 99;
        static public int Price_1_Month_comp { get; } = 119;
        static public int Price_3_Month_mobile { get; } = 249;
        static public int Price_3_Month_comp { get; } = 299;
        static public int Comp_CountINServer { get; set; } = 40; // максимум количесвто человек на сервере
        static public int CountINServerIpSec { get; set; } = 50; // максимум количесвто человек на сервере
        static public int CountINServerSocks { get; set; } = 25; // максимум количесвто человек на сервере
        static public string PromocodeName { get; set; } = "DE";// промокод для участия в акциях
        static public bool PROMOCODE_MODE { get; set; } = true; // переменная для включения и отключения действия промокода
        static public int USERS_COMP { get; set; } = 1000; // число на которое умножается chaid пользователя для получения id для компа для этого пользователя


        /// <summary>
        /// Список доступных серваков ipsec ПОРЯДОК ВАЖЕН ТАК КАК СОЗДАЮТСЯ NAMECERTAIN В КАЖДОМ КЛАССЕ
        /// </summary>
        public static readonly string[] Comp_IPSEC_SERVERS_LIST = { "Comp_IPSEC_1", "Comp_IPSEC_2", "Comp_IPSEC_3" };
        /// <summary>
        /// Список доступных серваков ipsec ПОРЯДОК ВАЖЕН ТАК КАК СОЗДАЮТСЯ NAMECERTAIN В КАЖДОМ КЛАССЕ
        /// </summary>
        public static readonly string[]  IPSEC_SERVERS_LIST 
            = { "IPSEC_1", "IPSEC_2", "IPSEC_3", "IPSEC_4", "IPSEC_5", "IPSEC_6", "IPSEC_7", "IPSEC_8", "IPSEC_9", "IPSEC_10", "IPSEC_11",
                 "IPSEC_12", "IPSEC_13", "IPSEC_14"};
        /// <summary>
        /// Список доступных серваков socks ПОРЯДОК ВАЖЕН ТАК КАК СОЗДАЮТСЯ NAMECERTAIN В КАЖДОМ КЛАССЕ
        /// </summary>
        public static readonly string[] SOCKS_SERVERS_LIST = { "SOCKS_1", "SOCKS_2", "SOCKS_3", "SOCKS_4", "SOCKS_5", "SOCKS_6" };

        // Перечисления для определения типа подключения бесплатный или платный
        public enum TypeConnect { Free, Payment};

        /// <summary>
        /// Метод обработки сообщений от пользователей бота
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Telegram.Bot.Types.Update update, CancellationToken cancellationToken)
        {
            try
            {
                #region FIX_MODE
                if (BOT_FIX_MODE)
                {
                    var message = update.Message; // (текст, стикер, картинка и тд) 
                    var button = update.CallbackQuery; //  inline кнопки                    

                    if (message != null)
                    {
                        if(message.Text != null)
                            if (message.Text.ToLower() == "/bot_fix_mode_false")
                            {
                                if (message.Chat.Id == 1278048494) // только я могу переводить в режим починки
                                {
                                    await botClient.SendTextMessageAsync(message.Chat.Id, "Бот в режиме LIVE.");
                                    BOT_FIX_MODE = false;
                                }
                                return;
                            }
                        if (message.Chat.Id > 0)
                        {
                            await botClient.SendTextMessageAsync(message.Chat, "Бот временно недоступен 😵\n Проходят технические настройки 🛠");

                        }
                    }
                    if (button != null && button.Message.Chat.Id > 0)
                        await botClient.SendTextMessageAsync(button.Message.Chat.Id, "Бот временно недоступен 😵.\n Проходят технические настройки 🛠");
                    return;
                }
                #endregion

                var SocksResolver = Program.HostApp.Services.GetRequiredService<SOCKSServiceResolver>();
                var Socks1Service = SocksResolver(SOCKS_SERVERS_LIST[0]); // получаю базовый класс

                var IPSecResolver = Program.HostApp.Services.GetRequiredService<IPSecServiceResolver>();
                var CompIPSecResolver = Program.HostApp.Services.GetRequiredService<Comp_IPSecServiceResolver>();
                //var IPSec1Service = IPSecResolver(IPSEC_SERVERS_LIST[1]); // получаю базовый класс

                var botComands = Program.HostApp.Services.GetRequiredService<IBotCommands>();
                var comp_Comands = Program.HostApp.Services.GetRequiredService<ICompMethods>();
                var mobile_Comands = Program.HostApp.Services.GetRequiredService<IMobileMethods>();

                /// <summary>
                /// Если апдейт от пользователя представляет сообщение (текст, стикер, картинка и тд) 
                /// </summary>
                if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
                {
                    Log.Information("Пользователь: {firstname} chatid: {chatid} text: {text}",
                        update?.Message?.From?.FirstName, update?.Message?.Chat.Id, update?.Message?.Text);

                    var message = update.Message;

                    if (message.Chat.Id < 0) // защита от отрицательного ID (группы, супергруппы имеют отриц ID)
                        return;

                    //if (message.Chat.Id != 1278048494) // для разработки
                    //{
                    //    await botClient.SendTextMessageAsync(message.Chat, "Бот временно недоступен 😵\n Вернемся в ближайшее время 🛠");
                    //    return;
                    //}

                    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    /// <summary>
                    /// Если произошла успешная оплата
                    /// </summary>
                    if (message.Type == Telegram.Bot.Types.Enums.MessageType.SuccessfulPayment)
                    {
                        var payment = message.SuccessfulPayment;

                        var match_ = Regex.Match(payment.InvoicePayload, $@"{NamesInlineButtons.ContinuePayment}.*");
                        if (match_.Success) // продление компов и телефонов
                        {
                            await botComands.BotContinuePaymentAsync(botClient, update.Message.Chat.Id, payment); // Продление подписки уже активным пользователям
                        }
                        else
                        {
                            var match_comp = Regex.Match(payment.InvoicePayload, $@"{NamesInlineButtons.Comp_Payment}.*");
                            if (match_comp.Success) // платные компы
                            {
                                await comp_Comands.Comp_NewPaymentAsync(botClient, payment, update); //Создание пользователя и занесение в БД если ты новый пользователь или неактивный
                            }
                            else // платные телефоны
                            {
                                await mobile_Comands.Mobile_NewPaymentAsync(botClient, payment, update); //Создание пользователя и занесение в БД если ты новый пользователь или неактивный
                            }
                        }

                        return;
                    }
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    #region BotCommands

                    /// <summary>
                    /// Если апдейт от пользователя представляет не текст (стикер, видео, картинка и тд)
                    /// </summary>
                    if (message.Text == null)
                    {
                        await botClient.SendTextMessageAsync(message.Chat, "Выберите /start в меню для начала работы с ботом");
                        return;
                    }

                    /// <summary>
                    /// Если сообщение от пользователя /start. Начало работы и показ основных возможностей бота, формирование счета на оплату(invoice)
                    /// </summary>
                    if (message.Text.ToLower() == "/start")
                    {
                        //await botComands.BotStartAsync(botClient, message.Chat.Id);
                        await botComands.BotStartBotSelectTypeDeviceAsync(botClient, message.Chat.Id);
                        return;
                    }

                    /// <summary>
                    /// Если сообщение от пользователя /about_me. Рассказываем о сервисе и возможностях бота
                    /// </summary>
                    if (message.Text.ToLower() == "/about_me")
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, $"Привет, я бот {BotName} — надежный сервис для подключения к сети Интернет без рекламы и ограничения трафика 🔥. \n\n" +
                            $"Работа нашего сервиса построена на современном протоколе IPsec IKeV2 со сквозным RSA–шифрованием. Также имеется решение основанное на протоколе Shadowsocks – это сетевой протокол" +
                            $" маскирования передачи данных, с открытым исходным кодом и развитый на базе технологии SOCKS5. ⚡️\n" +
                            $"Мы предлагаем стабильное и высокоскоростное подключение к нашим серверам в Нидерландах 🇳🇱 и Израиле 🇮🇱 за 99 руб / мес.\n\n" +
                            $"Действует реферальная система 👥. Поддержка — всегда на связи 🤖 (см.меню).");
                        return;
                    }

                    /// <summary>
                    /// Если сообщение от пользователя /referal. Рассказываем о реферальной программе пользователей
                    /// </summary>
                    if (message.Text.ToLower() == "/referal")
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, $"Привет, это описание реферальной программы! ✨\n" +
                            $"(Применима только к сервисам мобильных устройств)*\n\n" +
                            $"Подключи к нам 3 друзей и получи 3 месяца бесплатного пользования.🏅\n\n" +
                            $"Что для этого необходимо сделать?\n\n" +
                            $"— Приглашающему:  🌝\n" +
                            $"0. Быть активным пользователем.\n" +
                            $"1. Узнать свой ID (используй команду /get_chat_id).\n" +
                            $"2. Передать его другу, который хочет подключиться.\n\n" +
                            $"— Другу:  🌚\n" +
                            $"1. Оформить подписку.\n" +
                            $"2. Прислать чат - боту {BotName} сообщение вида:\n" +
                            $"''/ref_friend [ID-номер приглашающего]''.\n\n" +
                            $"Вуаля, шлём вам лучи признательности! 😊");
                        return;
                    }

                    /// <summary>
                    /// Если сообщение от пользователя /support. Обратаная связь для решения вопросов.
                    /// </summary>
                    if (message.Text.ToLower() == "/support")
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, $"Привет, это поддержка телеграм бота {BotName}!\n\n" +
                            "Если есть какие-то вопросы, то напиши нам в телеге ✍️ @NamelessVPN_support, поможем, расскажем, ответим на вопросы! 👨‍💻");
                        return;
                    }
                  
                    /// <summary>
                    /// Если сообщение от пользователя /get_chat_id. Получение уникального chat id пользователя
                    /// </summary>
                    if (message.Text.ToLower() == "/get_chat_id")
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, $"Ваш Chat ID: {message.Chat.Id}");
                        return;
                    }

                    /// <summary>
                    /// Если сообщение от пользователя /ref_friend. добавляю 
                    /// </summary>
                    if (message.Text.ToLower().Contains("/ref_friend "))
                    {
                        var match_referal = Regex.Match(message.Text.ToLower(), @"/ref_friend (\d+)");
                        if (match_referal.Success)
                        {
                            long.TryParse(match_referal.Groups[1].Value, out long clientID); // chatId пользователя за которого голосуют
                            await botComands.BotAddReferalProgramAsync(botClient, message.Chat.Id, clientID);
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(message.Chat.Id, $"Данные введены некорректно 😢\nПример: /ref_friend 1234567");
                        }
                        return;
                    }

                    /// <summary>
                    /// Если сообщение от пользователя /about_promo. Описание использования промокода
                    /// </summary>
                    if (message.Text.ToLower() == "/about_promo")
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, $"Привет, здесь написано как использовать промокод! 🎟\n\n" +
                            $"Отправь боту свой промокод и следуй дальнейшим инструкциям 🏆");
                        return;
                    }


                    /// <summary>
                    /// Если сообщение от пользователя /switch_on_promo. Пользователь использует промокод
                    /// </summary>
                    if (message.Text.ToLower() == PromocodeName.ToLower()) // если промокоды совпадают
                    {
                        if (PROMOCODE_MODE) // если режим промокода включен
                        {
                            var replyMarkup = new InlineKeyboardMarkup(new[]
                            {
                                new[]
                                {
                                    InlineKeyboardButton.WithUrl("Инженерообязанный", "https://t.me/halltape_data"),
                                },
                                new[]
                                {
                                    InlineKeyboardButton.WithUrl("Я – Дата Инженер", "https://t.me/Shust_DE")
                                },
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData("Активировать промокод", "promocode_on")
                                }
                            });
                            var messagepromo = await botClient.SendTextMessageAsync(message.Chat.Id, "❗ Чтобы использовать промокод необходимо *подписаться* ✅ на оба канала, а также быть активным пользователем нашего сервиса.",
                                replyMarkup: replyMarkup, parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
                        }
                        else
                            await botClient.SendTextMessageAsync(message.Chat.Id, $"Этот промокод устарел 😢");

                        return;
                    }

                    #endregion
                    //////////////////////////////////////////
                    #region Admin Commands

                    /// <summary>
                    /// Если сообщение от пользователя /send_message_to. Отправляю сообщение конкретному пользователю для проверки связи 
                    /// </summary>
                    if (message.Text.ToLower() == "/bot_current_limit_types")
                    {
                        await botClient.SendTextMessageAsync(message.Chat, $"Ограничения:\n" +
                            $"IPSEC - {CountINServerIpSec}\n" +
                            $"SOCKS - {CountINServerSocks}\n" +
                            $"COMP - {Comp_CountINServer}");
                        return;
                    }

                    /// <summary>
                    /// Если сообщение от пользователя /send_message_to. Отправляю сообщение конкретному пользователю для проверки связи 
                    /// </summary>
                    var match_send_messahe = Regex.Match(message.Text.ToLower(), @"/send_message_to (\d+)");
                    if (match_send_messahe.Success)
                    {
                        long.TryParse(match_send_messahe.Groups[1].Value, out long clientID);
                        await botClient.SendTextMessageAsync(clientID, $"Проверка связи прошла успешно! 🤙");
                        return;
                    }

                    /// <summary>
                    /// Если сообщение от пользователя /send_message_to_user. Отправляю сообщение юзеру
                    /// </summary>
                    var match_send_messahe_to_user = Regex.Match(message.Text, @"/send_message_to_user (\d+)((.*\n{0,}){0,})");
                    if (match_send_messahe_to_user.Success)
                    {
                        long.TryParse(match_send_messahe_to_user.Groups[1].Value, out long clientID);
                        string text = match_send_messahe_to_user.Groups[2].Value;
                        try
                        {
                            await botClient.SendTextMessageAsync(clientID, text);
                            await botClient.SendTextMessageAsync(message.Chat.Id, $"Текст отправлен пользователю");
                        }
                        catch (Exception exep) {
                            await botClient.SendTextMessageAsync(message.Chat.Id, $"Текст не доставлен, ошибка {exep}");
                        }

                        return;
                    }

                    /// <summary>
                    /// Если сообщение от пользователя /send_message_to_server_ipsec. Отправляю сообщение юзеру
                    /// </summary>
                    var send_message_to_server_ipsec = Regex.Match(message.Text, @"/send_message_to_server_ipsec (\d+)((.*\n{0,}){0,})");
                    if (send_message_to_server_ipsec.Success)
                    {
                        int.TryParse(send_message_to_server_ipsec.Groups[1].Value, out int serverID);
                        string text = send_message_to_server_ipsec.Groups[2].Value;

                        if (text == "")
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Текст пуст и не отправлен пользователям");
                        else
                        {
                            var count = await botComands.BotSendMessageToUsersInDBAsync(botClient, "server_ipsec", text, serverID);
                            await botClient.SendTextMessageAsync(message.Chat.Id, $"Текст отправлен {count} пользователям");
                        }

                        return;
                    }

                    /// <summary>
                    /// Если сообщение от пользователя /send_message_to_user. Отправляю сообщение юзеру
                    /// </summary>
                    var send_message_to_server_socks = Regex.Match(message.Text, @"/send_message_to_server_socks (\d+)((.*\n{0,}){0,})");
                    if (send_message_to_server_socks.Success)
                    {
                        int.TryParse(send_message_to_server_socks.Groups[1].Value, out int serverID);
                        string text = send_message_to_server_socks.Groups[2].Value;

                        if (text == "")
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Текст пуст и не отправлен пользователям");
                        else
                        {
                            var count = await botComands.BotSendMessageToUsersInDBAsync(botClient, "server_socks", text, serverID);
                            await botClient.SendTextMessageAsync(message.Chat.Id, $"Текст отправлен {count} пользователям");
                        }

                        return;
                    }

                    /// <summary>
                    /// Если сообщение от пользователя /send_message_to_IPSecUsers_ios. Отправляю сообщение всем АКТИВНЫМ пользователям IPSEC ios
                    /// </summary>
                    var match_send_message_to_IPSecUsers = Regex.Match(message.Text, @"/send_message_to_IPSecUsers_ios((.*\n{0,}){0,})");
                    if (match_send_message_to_IPSecUsers.Success)
                    {
                        string text = match_send_message_to_IPSecUsers.Groups[1].Value;
                        if(text == "")
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Текст пуст и не отправлен пользователям");
                        else
                        {
                            var count = await botComands.BotSendMessageToUsersInDBAsync(botClient, NamesInlineButtons.IPSEC_ios, text, 0);
                            await botClient.SendTextMessageAsync(message.Chat.Id, $"Текст отправлен {count} пользователям");
                        }                        

                        return;
                    }

                    /// <summary>
                    /// Если сообщение от пользователя /send_message_to_IPSecUsers_android. Отправляю сообщение всем АКТИВНЫМ пользователям IPSEC android
                    /// </summary>
                    var match_send_message_to_IPSecUsers_andr = Regex.Match(message.Text, @"/send_message_to_IPSecUsers_android((.*\n{0,}){0,})");
                    if (match_send_message_to_IPSecUsers_andr.Success)
                    {
                        string text = match_send_message_to_IPSecUsers_andr.Groups[1].Value;
                        if (text == "")
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Текст пуст и не отправлен пользователям");
                        else
                        {
                            var count = await botComands.BotSendMessageToUsersInDBAsync(botClient, NamesInlineButtons.IPSEC_android, text, 0);
                            await botClient.SendTextMessageAsync(message.Chat.Id, $"Текст отправлен {count} пользователям");
                        }                        

                        return;
                    }

                    /// <summary>
                    /// Если сообщение от пользователя /send_message_to_SocksUsers. Отправляю сообщение всем АКТИВНЫМ пользователям SOCKS
                    /// </summary>
                    var match_send_message_to_SocksUsers = Regex.Match(message.Text, @"/send_message_to_SocksUsers((.*\n{0,}){0,})");
                    if (match_send_message_to_SocksUsers.Success)
                    {
                        string text = match_send_message_to_SocksUsers.Groups[1].Value;
                        if (text == "")
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Текст пуст и не отправлен пользователям");
                        else
                        {
                            var count = await botComands.BotSendMessageToUsersInDBAsync(botClient, NamesInlineButtons.Socks, text, 0);
                            await botClient.SendTextMessageAsync(message.Chat.Id, $"Текст отправлен {count} пользователям");
                        }                       

                        return;
                    }

                    /// <summary>
                    /// Если сообщение от пользователя /send_message_to_AllUsers. Отправляю сообщение всем АКТИВНЫМ пользователям
                    /// </summary>
                    var match_send_message_to_AllUsers = Regex.Match(message.Text, @"/send_message_to_AllUsers((.*\n{0,}){0,})");
                    if (match_send_message_to_AllUsers.Success)
                    {
                        string text = match_send_message_to_AllUsers.Groups[1].Value;
                        if (text == "")
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Текст пуст и не отправлен пользователям");
                        else
                        {
                            var count = await botComands.BotSendMessageToUsersInDBAsync(botClient, NamesInlineButtons.AllUsers, text, 0);
                            await botClient.SendTextMessageAsync(message.Chat.Id, $"Текст отправлен {count} пользователям");
                        }                        

                        return;
                    }

                    /// <summary>
                    /// Если сообщение от пользователя /send_message_to_AllUsers. Отправляю сообщение всем НЕАКТИВНЫМ пользователям
                    /// </summary>
                    var match_send_message_to_NonActiveUsers = Regex.Match(message.Text, @"/send_message_to_NonActiveUsers((.*\n{0,}){0,})");
                    if (match_send_message_to_NonActiveUsers.Success)
                    {
                        string text = match_send_message_to_NonActiveUsers.Groups[1].Value;
                        if (text == "")
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Текст пуст и не отправлен пользователям");
                        else
                        {
                            var count = await botComands.BotSendMessageToUsersInDBAsync(botClient, "nonactive", text, 0);
                            await botClient.SendTextMessageAsync(message.Chat.Id, $"Текст отправлен {count} пользователям");
                        }

                        return;
                    }

                    /// <summary>
                    /// Если сообщение от пользователя /info_about_user. Получаю всю инфу о пользователе
                    /// </summary>
                    var match_info_about_user = Regex.Match(message.Text.ToLower(), @"/info_about_user (\d+)");
                    if (match_info_about_user.Success)
                    {
                        string result = string.Empty;
                        long.TryParse(match_info_about_user.Groups[1].Value, out long clientID);
                        
                        var user = await botComands.BotCheckUserBDAsync(clientID, 2); // есть ли ваще

                        if (user != null) 
                        {
                            result = $"ChatID: {user.ChatID}\n" +
                                $"Вид устройства: {user.TypeOfDevice}\n" +
                                $"Ник нейм: {user.FirstName}\n" +
                                $"Status: {user.Status}\n" +
                                $"Блатной: {user.Blatnoi}\n" +
                                $"Активен до: {user.DateNextPayment}\n" +
                                $"Сервис: {user.NameService}\n" +
                                $"ОС: {user.NameOS}\n" +
                                $"Ключ: {user.ServiceKey}\n" +
                                $"Адрес сервера: {user.ServiceAddress}\n\n";
                        }
                        else
                        {
                            result = $"Пользователь с chatID {clientID} отсутствует в БД";
                        }

                        var user_comp = await botComands.BotCheckUserBDAsync(clientID * USERS_COMP, 2); // есть ли ваще

                        if (user_comp != null)
                        {
                            result += $"ChatID: {user_comp.ChatID}\n" +
                                $"Вид устройства: {user_comp.TypeOfDevice}\n" +
                                $"Ник нейм: {user_comp.FirstName}\n" +
                                $"Status: {user_comp.Status}\n" +
                                $"Блатной: {user_comp.Blatnoi}\n" +
                                $"Активен до: {user_comp.DateNextPayment}\n" +
                                $"Сервис: {user_comp.NameService}\n" +
                                $"ОС: {user_comp.NameOS}\n" +
                                $"Ключ: {user_comp.ServiceKey}\n" +
                                $"Адрес сервера: {user_comp.ServiceAddress}";
                        }
                        else
                        {
                            result += $"Пользователь не использует сервис для ПК";
                        }

                        await botClient.SendTextMessageAsync(message.Chat.Id, result);
                        return;
                    }

                    /// <summary>
                    /// Если сообщение от пользователя /bot_fix_mode_true. Перевожу бота в режим починки
                    /// </summary>
                    if (message.Text.ToLower() == "/bot_fix_mode_true")
                    {
                        if (message.Chat.Id == 1278048494) // только я могу переводить в режим починки
                        {
                            BOT_FIX_MODE = true;
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Бот в режиме починки.");
                        }
                        return;
                    }

                    /// <summary>
                    /// Если сообщение от пользователя /bot_change_count_max_users_ipsec_comp_in_server. Изменяю макс кол-во пользователей на серваках ipsec
                    /// </summary>
                    var match_change_count_user_ipsec_comp = Regex.Match(message.Text.ToLower(), @"/bot_change_count_max_users_ipsec_comp_in_server (\d+)");
                    if (match_change_count_user_ipsec_comp.Success)
                    {
                        int.TryParse(match_change_count_user_ipsec_comp.Groups[1].Value, out int countUsersServer);

                        Comp_CountINServer = countUsersServer; // мняю мах кол-во пользователей на сервере 

                        await botClient.SendTextMessageAsync(message.Chat.Id, $"Успех, поменял на {Comp_CountINServer} для Компов");
                        return;
                    }

                    /// <summary>
                    /// Если сообщение от пользователя /bot_change_count_max_users_ipsec_in_server. Изменяю макс кол-во пользователей на серваках ipsec
                    /// </summary>
                    var match_change_count_user_ipsec = Regex.Match(message.Text.ToLower(), @"/bot_change_count_max_users_ipsec_in_server (\d+)");
                    if (match_change_count_user_ipsec.Success)
                    {
                        int.TryParse(match_change_count_user_ipsec.Groups[1].Value, out int countUsersServer);

                        CountINServerIpSec = countUsersServer; // мняю мах кол-во пользователей на сервере 

                        await botClient.SendTextMessageAsync(message.Chat.Id, $"Успех, поменял на {CountINServerIpSec} для IpSec");
                        return;
                    }

                    /// <summary>
                    /// Если сообщение от пользователя /bot_change_count_max_users_socks_in_server. Изменяю макс кол-во пользователей на серваках socks
                    /// </summary>
                    var match_change_count_user_socks = Regex.Match(message.Text.ToLower(), @"/bot_change_count_max_users_socks_in_server (\d+)");
                    if (match_change_count_user_socks.Success)
                    {
                        int.TryParse(match_change_count_user_socks.Groups[1].Value, out int countUsersServer);

                        CountINServerSocks = countUsersServer; // мняю мах кол-во пользователей на сервере 

                        await botClient.SendTextMessageAsync(message.Chat.Id, $"Успех, поменял на {CountINServerSocks} для Socks");
                        return;
                    }

                    /// <summary>
                    /// Если сообщение от пользователя /bot_promocode_mode_true. Перевожу бота в режим принятия промокода
                    /// </summary>
                    if (message.Text.ToLower() == "/bot_promocode_mode_true")
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, $"Режим промокода активирован");
                        await botClient.SendTextMessageAsync(1278048494, $"Режим промокода активирован");

                        PROMOCODE_MODE = true;
                        return;
                    }

                    /// <summary>
                    /// Если сообщение от пользователя /bot_promocode_mode_false. Перевожу бота в режим отключения промокодов
                    /// </summary>
                    if (message.Text.ToLower() == "/bot_promocode_mode_false")
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, $"Режим промокода выключен");
                        await botClient.SendTextMessageAsync(1278048494, $"Режим промокода выключен");

                        PROMOCODE_MODE = false;
                        return;
                    }

                    /// <summary>
                    /// Если сообщение от пользователя /admin_update_promo. Изменяю действующий промокод в боте
                    /// </summary>
                    var match_update_promocode = Regex.Match(message.Text.ToLower(), @"/admin_update_promo (\S+)");
                    if (match_update_promocode.Success)
                    {
                        PromocodeName = match_update_promocode.Groups[1].Value;

                        await botClient.SendTextMessageAsync(message.Chat.Id, $"Успех, поменял промокод на {PromocodeName}");
                        await botClient.SendTextMessageAsync(1278048494, $"Успех, поменял промокод на {PromocodeName}");
                        return;
                    }

                    /// <summary>
                    /// Если сообщение от пользователя /bot_current_promocode. Получаю действующий промокод в боте
                    /// </summary>
                    if (message.Text.ToLower() == "/bot_current_promocode")
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, $"Режим промокодов в боте: {PROMOCODE_MODE}\n" +
                            $"Текущий промокод: {PromocodeName}");
                        await botClient.SendTextMessageAsync(1278048494, $"Режим промокодов в боте: {PROMOCODE_MODE}\n" +
                            $"Текущий промокод: {PromocodeName}");
                        return;
                    }

                    /// <summary>
                    /// Если сообщение от пользователя /bot_take_off_promo_db. Убираю все галки в бд что пользователи использовали промокод 
                    /// </summary>
                    if (message.Text.ToLower() == "/bot_take_off_promo_db")
                    {
                        await botComands.BotTakeOffDBPromoAsync(botClient, message.Chat.Id);
                        return;
                    }

                    /// <summary>
                    /// Если сообщение от пользователя /bot_mark_blatnoi. Делаю пользователя блатным
                    /// </summary>
                    var match_mark_blatnoi = Regex.Match(message.Text.ToLower(), @"/bot_mark_blatnoi (\d+)");
                    if (match_mark_blatnoi.Success)
                    {
                        long.TryParse(match_mark_blatnoi.Groups[1].Value, out long clientID);
                        await botComands.BotMarkBlatnoiAsync(botClient, clientID, message.Chat.Id);
                        return;
                    }

                    /// <summary>
                    /// Если сообщение от пользователя /bot_add_days_user. Добавляю пользователю дни
                    /// </summary>
                    var match_add_days = Regex.Match(message.Text.ToLower(), @"/bot_add_days_user (\d+) days (\d+)");
                    if (match_add_days.Success)
                    {
                        long.TryParse(match_add_days.Groups[1].Value, out long clientID);
                        int.TryParse(match_add_days.Groups[2].Value, out int days);
                        await botComands.BotAddDaysToUserAsync(botClient, clientID, days, message.Chat.Id);
                        return;
                    }

                    /// <summary>
                    /// Если сообщение от пользователя /bot_remove_days_user. Удаляю пользователю дни
                    /// </summary>
                    var match_remove_days = Regex.Match(message.Text.ToLower(), @"/bot_remove_days_user (\d+) days (\d+)");
                    if (match_remove_days.Success)
                    {
                        long.TryParse(match_remove_days.Groups[1].Value, out long clientID);
                        int.TryParse(match_remove_days.Groups[2].Value, out int days);
                        await botComands.BotRemoveDaysToUserAsync(botClient, clientID, days, message.Chat.Id);
                        return;
                    }

                    /// <summary>
                    /// Если сообщение от пользователя /bot_add_days_user. Добавляю пользователю дни
                    /// </summary>
                    var match_shift_dates = Regex.Match(message.Text.ToLower(), @"/bot_shift_dates_users diapason (\d+) days (\d+)");
                    if (match_shift_dates.Success)
                    {
                        int.TryParse(match_shift_dates.Groups[1].Value, out int diapason);
                        int.TryParse(match_shift_dates.Groups[2].Value, out int days);
                        await botComands.BotShiftNearestUsersDateAsync(botClient, diapason, days, message.Chat.Id);
                        return;
                    }

                    /// <summary>
                    /// Если сообщение от пользователя /bot_add_days_user. Добавляю пользователю дни
                    /// </summary>
                    if (message.Text.ToLower() == "/bot_get_total_counts_users")
                    {
                        await botComands.BotGetAllCountUsersAsync(botClient, message.Chat.Id);
                        return;
                    }
                    #endregion
                    //////////////////////////////////////////
                    #region IPSec Features

                    /// <summary>
                    /// Если сообщение от пользователя /get_total_users_ipsec. Получение количества активных пользователей выбранного впн ipsec 
                    /// </summary>
                    var match_get_total_users_ipsec = Regex.Match(message.Text.ToLower(), @"/get_total_users_ipsec");
                    if (match_get_total_users_ipsec.Success)
                    {
                        foreach (var item in IPSEC_SERVERS_LIST)
                        {
                            try
                            {
                                var SelectedIPsec = IPSecResolver(item);
                                var total_users = await SelectedIPsec.GetTotalUserAsync();
                                await botClient.SendTextMessageAsync(message.Chat.Id, $"Количестов активных пользователей: {total_users} на сервере {item}\n" +
                                    $"IP: {SelectedIPsec.ServerIPSec}");
                            }
                            catch(Exception exx) 
                            {
                                await botClient.SendTextMessageAsync(message.Chat.Id, $"Не пашет {item}!");
                            }
                        }

                        foreach (var item in Comp_IPSEC_SERVERS_LIST)
                        {
                            try
                            {
                                var SelectedIPsec = CompIPSecResolver(item);
                                var total_users = await SelectedIPsec.GetTotalUserAsync();
                                await botClient.SendTextMessageAsync(message.Chat.Id, $"Количестов активных пользователей: {total_users} на сервере {item}\n" +
                                    $"IP: {SelectedIPsec.ServerIPSec}");
                            }
                            catch (Exception exx)
                            {
                                await botClient.SendTextMessageAsync(message.Chat.Id, $"Не пашет {item}!");
                            }
                        }
                        return;
                    }

                    /// <summary>
                    /// Если сообщение от пользователя /delete_user_ipsec. Удаляю клиента по ID из vpn ipsec на указанном сервере и делаю неактивным в бд
                    /// </summary>
                    var match_del_ipsec = Regex.Match(message.Text.ToLower(), @"/delete_user_ipsec (\d+) server (\d+)");
                    if (match_del_ipsec.Success)
                    {
                        long.TryParse(match_del_ipsec.Groups[1].Value, out long clientID);
                        long.TryParse(match_del_ipsec.Groups[2].Value, out long server_id);

                        using (TgVpnbotContext db = new TgVpnbotContext())
                        {
                            var user = await db.Users.FirstOrDefaultAsync(u => u.ChatID == clientID).ConfigureAwait(false);
                            var SelectedIPsec = IPSecResolver(IPSEC_SERVERS_LIST[server_id - 1]);

                            var total_users_before = await SelectedIPsec.GetTotalUserAsync();
                            await botClient.SendTextMessageAsync(message.Chat.Id, $"Сервер МОБИЛ количество активных пользователей ДО удаления: {total_users_before} на сервере {server_id}");

                            var resRevoke = await SelectedIPsec.RevokeUserAsync(clientID);
                            var resDelete = await SelectedIPsec.DeleteUserAsync(clientID);

                            if (user != null)
                            {
                                user.Status = "nonactive";
                                user.DateDisconnect = DateTime.UtcNow;

                                // Преобразование в UTC, если необходимо
                                //if (user.DateCreate.Kind == DateTimeKind.Unspecified)
                                //    user.DateCreate = DateTime.SpecifyKind(user.DateCreate, DateTimeKind.Utc);

                                //if (user.DateNextPayment.Kind == DateTimeKind.Unspecified)
                                //    user.DateNextPayment = DateTime.SpecifyKind(user.DateNextPayment, DateTimeKind.Utc);

                                //Console.WriteLine($"DateDisconnect Kind: {user.DateDisconnect?.Kind}"); // Проверка Kind
                                //Console.WriteLine($"DateCreate Kind: {user.DateCreate.Kind}"); // Проверка Kind
                                //Console.WriteLine($"DateNext Kind: {user.DateNextPayment.Kind}"); // Проверка Kind
                                //db.Users.Update(user);
                                await db.SaveChangesAsync();
                            }
                            await botClient.SendTextMessageAsync(1278048494, $"Info about IPsec {clientID}: {resDelete}"); // присылаю себе ответ по удалению
                            if (message.Chat.Id != 1278048494)
                                await botClient.SendTextMessageAsync(message.Chat.Id, $"Info about IPsec {clientID}: {resDelete}"); // присылаю ответ по удалению

                            var total_users = await SelectedIPsec.GetTotalUserAsync();
                            await botClient.SendTextMessageAsync(message.Chat.Id, $"Сервер МОБИЛ количество активных пользователей ПОСЛЕ удаления: {total_users} на сервере {server_id}");
                        }

                        return;
                    }

                    /// <summary>
                    /// Если сообщение от пользователя /delete_user_ipsec. Удаляю клиента по ID из vpn ipsec на указанном сервере и делаю неактивным в бд
                    /// </summary>
                    var match_del_ipsec_comp = Regex.Match(message.Text.ToLower(), @"/delete_user_ipsec_comp (\d+) server (\d+)");
                    if (match_del_ipsec_comp.Success)
                    {
                        long.TryParse(match_del_ipsec_comp.Groups[1].Value, out long clientID);
                        long.TryParse(match_del_ipsec_comp.Groups[2].Value, out long server_id);

                        using (TgVpnbotContext db = new TgVpnbotContext())
                        {
                            var comp_chatID = clientID * USERS_COMP;

                            var user = await db.Users.FirstOrDefaultAsync(u => u.ChatID == comp_chatID).ConfigureAwait(false);
                            var SelectedIPsec = CompIPSecResolver(Comp_IPSEC_SERVERS_LIST[server_id - 1]);

                            var total_users_before = await SelectedIPsec.GetTotalUserAsync();
                            await botClient.SendTextMessageAsync(message.Chat.Id, $"Сервер КОМПОВ количество активных пользователей ДО удаления: {total_users_before} на сервере {server_id}");

                            var resRevoke = await SelectedIPsec.RevokeUserAsync(comp_chatID);
                            var resDelete = await SelectedIPsec.DeleteUserAsync(comp_chatID);

                            if (user != null)
                            {
                                DateTime? datetemp = DateTime.Now;
                                user.Status = "nonactive";
                                user.DateDisconnect = DateTime.UtcNow;

                                //db.Users.Update(user);
                                await db.SaveChangesAsync();
                            }
                            await botClient.SendTextMessageAsync(1278048494, $"Info about IPsec {comp_chatID}: {resDelete}"); // присылаю себе ответ по удалению
                            if (message.Chat.Id != 1278048494)
                                await botClient.SendTextMessageAsync(message.Chat.Id, $"Info about IPsec {comp_chatID}: {resDelete}"); // присылаю ответ по удалению

                            var total_users = await SelectedIPsec.GetTotalUserAsync();
                            await botClient.SendTextMessageAsync(message.Chat.Id, $"Сервер КОМПОВ количество активных пользователей ПОСЛЕ удаления: {total_users} на сервере {server_id}");
                        }

                        return;
                    }

                    /// <summary>
                    /// Если сообщение от пользователя /create_user_ipsec. Создаю клиента по ID из vpn ipsec для мобилы и добавляю в бд
                    /// </summary>
                    var match_create_ipsec = Regex.Match(message.Text.ToLower(), @"/create_user_ipsec (\d+) server (\d+) os (\d+)");
                    if (match_create_ipsec.Success)
                    {
                        long.TryParse(match_create_ipsec.Groups[1].Value, out long clientIDCreate);
                        int.TryParse(match_create_ipsec.Groups[2].Value, out int server_id);
                        long.TryParse(match_create_ipsec.Groups[3].Value, out long os); // ios = 1 android = 0
                        
                        using (TgVpnbotContext db = new TgVpnbotContext())
                        {
                            var user = await db.Users.FirstOrDefaultAsync(u => u.ChatID == clientIDCreate).ConfigureAwait(false);

                            if (user != null) // если пользователь уже есть но он не активный
                            {
                                user.Status = "active";

                                Log.Information("Админ create_client_ipsec, активировал старого пользователя, chatid: {chatid}, ОС: {os}", clientIDCreate, os);
                            }
                            else // если новый пользователь
                            {
                                user = new UserDB
                                {
                                    ChatID = clientIDCreate,
                                };

                                await db.Users.AddAsync(user);

                                Log.Information("Админ create_client_ipsec, новый пользователь добавлен, chatid: {chatid}, ОС: {os}", clientIDCreate, os);
                            }

                            IIPsec1 SelectedIPsec = null;

                            if(os == 1) // ios возвращаю
                            {
                                SelectedIPsec = await mobile_Comands.Mobile_CreateAndSendConfig_IpSec_IOS(botClient, clientIDCreate, server_id); // Отправка данных на сервер и формирование конфига. Отсылка готового конфига пользователю
                                user.NameOS = NamesInlineButtons.IOS;
                                user.TypeOfDevice = NamesInlineButtons.StartMobile;
                            }
                            else // android
                            {
                                SelectedIPsec = await mobile_Comands.Mobile_CreateAndSendConfig_IpSec_Android(botClient, clientIDCreate, server_id); // Отправка данных на сервер и формирование конфига. Отсылка готового конфига пользователю
                                user.NameOS = NamesInlineButtons.Android;
                                user.TypeOfDevice = NamesInlineButtons.StartMobile;
                            }

                            user.NameService = SelectedIPsec.NameCertainIPSec;
                            user.ServiceKey = clientIDCreate;
                            user.ServiceAddress = SelectedIPsec.ServerIPSec;

                            await db.SaveChangesAsync();
                        }

                        return;
                    }

                    /// <summary>
                    /// Если сообщение от пользователя /create_user_ipsec_comp. Создаю клиента по ID из vpn ipsec для компа и добавляю в бд
                    /// </summary>
                    var match_create_ipsec_comp = Regex.Match(message.Text.ToLower(), @"/create_user_ipsec_comp (\d+) server (\d+) os (\d+)");
                    if (match_create_ipsec_comp.Success)
                    {
                        long.TryParse(match_create_ipsec_comp.Groups[1].Value, out long clientIDCreate);
                        int.TryParse(match_create_ipsec_comp.Groups[2].Value, out int server_id);
                        long.TryParse(match_create_ipsec_comp.Groups[3].Value, out long os); // MacOS = 1 Windows = 0

                        using (TgVpnbotContext db = new TgVpnbotContext())
                        {
                            var comp_chatID = clientIDCreate * USERS_COMP;
                            var user = await db.Users.FirstOrDefaultAsync(u => u.ChatID == comp_chatID).ConfigureAwait(false);

                            if (user != null) // если пользователь уже есть но он не активный
                            {
                                user.Status = "active";

                                Log.Information("Админ create_client_ipsec, активировал старого пользователя, chatid: {chatid}, ОС: {os}", comp_chatID, os);
                            }
                            else // если новый пользователь
                            {
                                user = new UserDB
                                {
                                    ChatID = comp_chatID,
                                };

                                await db.Users.AddAsync(user);

                                Log.Information("Админ create_client_ipsec, новый пользователь добавлен, chatid: {chatid}, ОС: {os}", comp_chatID, os);
                            }

                            IIPsec1 SelectedIPsec = null;

                            if (os == 1) // MacOS возвращаю
                            {
                                SelectedIPsec = await comp_Comands.Comp_CreateAndSendConfig_IpSec_MacOS(botClient, clientIDCreate, server_id); // Отправка данных на сервер и формирование конфига. Отсылка готового конфига пользователю
                                user.NameOS = NamesInlineButtons.MacOS;
                                user.TypeOfDevice = NamesInlineButtons.StartComp;
                            }
                            else // Windows
                            {
                                SelectedIPsec = await comp_Comands.Comp_CreateAndSendConfig_IpSec_Windows(botClient, clientIDCreate, server_id); // Отправка данных на сервер и формирование конфига. Отсылка готового конфига пользователю
                                user.NameOS = NamesInlineButtons.Windows;
                                user.TypeOfDevice = NamesInlineButtons.StartComp;
                            }

                            user.NameService = SelectedIPsec.NameCertainIPSec;
                            user.ServiceKey = comp_chatID;
                            user.ServiceAddress = SelectedIPsec.ServerIPSec;

                            await db.SaveChangesAsync();
                        }

                        return;
                    }

                    #endregion
                    //////////////////////////////////////////
                    #region Socks Features

                    var match_create_socks = Regex.Match(message.Text.ToLower(), @"/create_user_socks (\d+) server (\d+) os (\d+)");
                    if (match_create_socks.Success)
                    {
                        long.TryParse(match_create_socks.Groups[1].Value, out long clientID);
                        int.TryParse(match_create_socks.Groups[2].Value, out int server_id);
                        long.TryParse(match_create_socks.Groups[3].Value, out long os);

                        (ISocks1 socksServer, long id_newUser) resFunc = (null, 0);

                        using (TgVpnbotContext db = new TgVpnbotContext())
                        {
                            var user = await db.Users.FirstOrDefaultAsync(u => u.ChatID == clientID).ConfigureAwait(false);

                            if (user != null) // если пользователь уже есть но он не активный
                            {
                                user.Status = "active";

                                Log.Information("Админ create_user_socks, активировал старого пользователя, chatid: {chatid}, ОС: {os}", clientID, os);
                            }
                            else // если новый пользователь
                            {
                                user = new UserDB
                                {
                                    ChatID = clientID,
                                };

                                await db.Users.AddAsync(user);

                                Log.Information("Админ create_user_socks, новый пользователь добавлен, chatid: {chatid}, ОС: {os}", clientID, os);
                            }

                            
                            if (os == 1) // ios возвращаю
                            {
                                resFunc = await mobile_Comands.Mobile_CreateAndSendKey_Socks(botClient, clientID, NamesInlineButtons.IOS, server_id);
                                user.NameOS = NamesInlineButtons.IOS;
                            }
                            else // android
                            {
                                resFunc = await mobile_Comands.Mobile_CreateAndSendKey_Socks(botClient, clientID, NamesInlineButtons.Android, server_id);
                                user.NameOS = NamesInlineButtons.Android;
                            }

                            user.NameService = resFunc.socksServer.NameCertainSocks;
                            user.ServiceKey = resFunc.id_newUser;
                            user.ServiceAddress = resFunc.socksServer.ServerSocks;

                            await db.SaveChangesAsync();
                        }                       
                        return;
                    }

                    /// <summary>
                    /// Если сообщение от пользователя /delete_client_socks. Удаляю клиента по ID из vpn socks
                    /// </summary>
                    var match_del_socks = Regex.Match(message.Text.ToLower(), @"/delete_user_socks (\d+) server (\d+) keyid (\d+)");
                    if (match_del_socks.Success)
                    {
                        long.TryParse(match_del_socks.Groups[1].Value, out long clientID);
                        long.TryParse(match_del_socks.Groups[2].Value, out long server_id);
                        long.TryParse(match_del_socks.Groups[3].Value, out long key_id);

                        using (TgVpnbotContext db = new TgVpnbotContext())
                        {
                            var user = await db.Users.FirstOrDefaultAsync(u => u.ChatID == clientID).ConfigureAwait(false);

                            var SelectedSocks = SocksResolver(SOCKS_SERVERS_LIST[server_id - 1]);
                            var res = await SelectedSocks.DeleteUserAsync(key_id);

                            if (user != null)
                            {
                                DateTime? datetemp = DateTime.Now;
                                user.Status = "nonactive";
                                user.DateDisconnect = DateTime.UtcNow;

                                //db.Users.Update(user);
                                await db.SaveChangesAsync();
                            }
                            await botClient.SendTextMessageAsync(1278048494, $"Info about Socks {clientID}: {res}"); // присылаю себе ответ по удалению
                            if (message.Chat.Id != 1278048494)
                                await botClient.SendTextMessageAsync(message.Chat.Id, $"Info about Socks {clientID}: {res}"); // присылаю себе ответ по удалению
                        }

                        return;
                    }

                    /// <summary>
                    /// Если сообщение от пользователя /get_total_users_socks. Получение количества активных пользователей shadowsocks на выбранном сервере
                    /// </summary>
                    var match_get_total_users_socks = Regex.Match(message.Text.ToLower(), @"/get_total_users_socks");
                    if (match_get_total_users_socks.Success)
                    {
                        foreach (var item in SOCKS_SERVERS_LIST)
                        {
                            try
                            {
                                var SelectedServer = SocksResolver(item);
                                string filename = $"users_{item}.csv";
                                var res = await SelectedServer.GetFileUsersAsync(filename, true);
                                await using Stream stream = System.IO.File.OpenRead($@"../{filename}");

                                await botClient.SendDocumentAsync(
                                            chatId: message.Chat,
                                            document: new InputOnlineFile(content: stream, fileName: $"{filename}"));

                                await botClient.SendTextMessageAsync(message.Chat.Id, $"IP {SelectedServer.ServerSocks}");
                            }
                            catch (Exception ex)
                            {
                                await botClient.SendTextMessageAsync(message.Chat.Id, $"Не пашет {item}!");
                            }
                        }
                        return;
                    }

                    /// <summary>
                    /// Если сообщение от пользователя /set_limit_traffic_socks. Установка ограничения лимита трафика всем пользователям shadowsocks
                    /// </summary>
                    var match_linit_traffic = Regex.Match(message.Text.ToLower(), @"/set_limit_traffic_socks (\d+)");
                    if (match_linit_traffic.Success)
                    {
                        long.TryParse(match_linit_traffic.Groups[1].Value, out long volume_traffic_gb);
                        var res = await Socks1Service.SetLimitTrafficAsync(volume_traffic_gb);

                        await botClient.SendTextMessageAsync(1278048494, $"Установка лимита трафика на носки: {res}"); // присылаю себе ответ по установке лимита трафика

                        return;
                    }

                    /// <summary>
                    /// Если сообщение от пользователя /delete_limit_traffic_socks. Удаление лимита ограничения трафика всем пользователям shadowsocks
                    /// </summary>
                    if (message.Text.ToLower() == "/delete_limit_traffic_socks")
                    {
                        var res = await Socks1Service.DeleteLimitTrafficAsync();

                        await botClient.SendTextMessageAsync(1278048494, $"Удаление лимита трафика на носки: {res}"); // присылаю себе ответ по удалнию лимита трафика
                        return;
                    }

                    #endregion


                    await botClient.SendTextMessageAsync(message.Chat, "Выберите /start в меню для начала работы с ботом или обратитесь в поддержку /support");
                }

                /// <summary>
                /// Если апдейт от пользователя представляет информацию о предварительной оплате подписки
                /// </summary>
                if (update.Type == Telegram.Bot.Types.Enums.UpdateType.PreCheckoutQuery)
                {
                    Log.Information("Предварительная оплата chatid: {chatid}, precheckoutquery: {precheckoutquery}",
                        update?.PreCheckoutQuery.From.Id, update.PreCheckoutQuery.Id);

                    await botClient.AnswerPreCheckoutQueryAsync(update.PreCheckoutQuery.Id);
                    return;
                }

                /// <summary>
                /// Если апдейт от пользователя представляет inline кнопки
                /// </summary>
                if (update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery)
                {
                    var button = update.CallbackQuery;

                    if (button.Message.Chat.Id < 0) // защита от отрицательного ID (группы, супергруппы имеют отриц ID)
                        return;

                    //if (button.Message.Chat.Id != 1278048494) // для теста
                    //{
                    //    await botClient.SendTextMessageAsync(button.Message.Chat.Id, "Бот временно недоступен 😵\n Вернемся в ближайшее время 🛠");
                    //    return;
                    //}

                    var real_chatId = button.Message.Chat.Id;
                    var comp_chatId = real_chatId * TgBotHostedService.USERS_COMP;

                    Log.Information("Нажата кнопка {inlineButton} пользователь: {firstName}, chatid: {chatid}", button.Data, button.Message.Chat?.FirstName, real_chatId);

                    if (button.Data == NamesInlineButtons.StartRouter)
                    {
                        await botClient.SendTextMessageAsync(real_chatId,
                            $"Откройте для себя мир без границ с нашими роутерами с встроенным VPN! 🚀\n\n" +
                            $"1. Неограниченный доступ к контенту : С нашими роутерами вы сможете наслаждаться любимыми видео на 🔴 YouTube" +
                            $" прямо на телевизоре, а также просматривать контент 📱 в Instagram, TikTok, Twitter и других социальных сетях без каких-либо ограничений." +
                            $" Забудьте о блокировках и географических ограничениях!\n\n" +
                            $"2. Удобный бот для управления VPN 🤖: Мы предлагаем вам инновационное решение — удобного бота, который позволяет включать и отключать VPN всего одной командой.\n\n" +
                            $"3. Простота подключения 🔌: Настройка роутера не займет у вас много времени. Просто вставьте шнур, и устройство начнет работать.\n\n" +
                            $"4. Безопасность и конфиденциальность 🔒: Встроенный VPN обеспечивает высокий уровень безопасности ваших данных, защищая вашу личную информацию от посторонних глаз.\n\n" +
                            $"За подробной информацией и заказом роутера пишите 🖊 в саппорт @NamelessVPN_support");
                        return;
                    }

                    if (button.Data == "promocode_on")
                    {
                        var usr_promo = await botComands.BotCheckUserBDAsync(real_chatId, 1);  // активный ли 

                        if (usr_promo != null) // пользователь есть в бд и активный
                        {
                            var isMember1 = await IsUserMemberOfChannel(botClient, real_chatId, -1002028974001);
                            var isMember2 = await IsUserMemberOfChannel(botClient, real_chatId, -1001525634239);

                            if (isMember1 && isMember2)
                            {
                                await botClient.DeleteMessageAsync(real_chatId, button.Message.MessageId);
                                await botClient.SendTextMessageAsync(real_chatId, "Спасибо, что подписаны на каналы! 🏆");
                                await botComands.BotCheckAndUsePromoAsync(botClient, real_chatId);
                            }
                            else
                            {
                                var rejectpromo = await botClient.SendTextMessageAsync(real_chatId,
                                        "*Вы не подписаны* на один или оба канала 😢.\n" +
                                        "Пожалуйста, подпишитесь ✅ и нажмите кнопку *Активировать промокод*",
                                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
                            }
                        }
                        else
                            await botClient.SendTextMessageAsync(real_chatId, "Для того чтобы применить промокод необходимо быть активным пользователем.\n\n" +
                                "Выберите /start в меню для начала работы с ботом 👍");

                        return;
                    }

                    // SELECT PERIOD CONTINUE_PAYMENT Mobile 1 month
                    if (button.Data == NamesInlineButtons.ContinuePayment_Mobile_1_month)
                    {
                        await botComands.BotCheckStatusUserAndSendContinuePayInvoice(botClient, button.Message.Chat.Id, NamesInlineButtons.ContinuePayment_Mobile_1_month, 1, NamesInlineButtons.StartMobile);
                        return;
                    }

                    // SELECT PERIOD CONTINUE_PAYMENT Mobile 3 month
                    if (button.Data == NamesInlineButtons.ContinuePayment_Mobile_3_month)
                    {
                        await botComands.BotCheckStatusUserAndSendContinuePayInvoice(botClient, button.Message.Chat.Id, NamesInlineButtons.ContinuePayment_Mobile_3_month, 3, NamesInlineButtons.StartMobile);
                        return;
                    }

                    // SELECT PERIOD CONTINUE_PAYMENT Comp 1 month
                    if (button.Data == NamesInlineButtons.ContinuePayment_Comp_1_month)
                    {
                        await botComands.BotCheckStatusUserAndSendContinuePayInvoice(botClient, button.Message.Chat.Id, NamesInlineButtons.ContinuePayment_Comp_1_month, 21, NamesInlineButtons.StartComp);
                        return;
                    }

                    // SELECT PERIOD CONTINUE_PAYMENT Comp 3 month
                    if (button.Data == NamesInlineButtons.ContinuePayment_Comp_3_month)
                    {
                        await botComands.BotCheckStatusUserAndSendContinuePayInvoice(botClient, button.Message.Chat.Id, NamesInlineButtons.ContinuePayment_Comp_3_month, 23, NamesInlineButtons.StartComp);
                        return;
                    }

                    var usr = await botComands.BotCheckUserBDAsync(real_chatId, 1);  // активный ли пользователь MOBILE

                    if (usr != null) // АКТИВНЫЙ ЮЗЕР НЕ МОЖЕТ НАЖАТЬ НИКУДА
                    {
                        await botClient.SendTextMessageAsync(real_chatId,
                            $"Ты уже являешься активным пользователем нашего сервиса для мобильного устройства 🏆 \n\n" +
                            $"Твоя подписка действует до {usr.DateNextPayment.ToString("dd-MM-yyyy")} 🤝 \n" +
                            $"Ты получишь уведомления от бота за 3,2,1 день до окончания подписки, а также возможность продлить её \n\n" +
                            $"Если возникли какие-либо вопросы, то ты всегда можешь написать в поддержку /support 🥸");
                    }
                    else // неактивный юзер или отсутствует в БД
                    {
                        // START BOT MOBILE
                        if (button.Data == NamesInlineButtons.StartMobile)
                        {
                            await mobile_Comands.Mobile_BotStartAsync(botClient, real_chatId);
                            return;
                        }

                        var userDB = await botComands.BotCheckUserBDAsync(real_chatId, 2);  // есть ли ваще в бд

                        if(userDB == null) // если пользователя нет в БД, то он может нажимать на бесплатные кнопки
                        {
                            // START Free period BUTTON MOBILE
                            if (button.Data == NamesInlineButtons.Mobile_TryFreePeriod)
                            {
                                await mobile_Comands.Mobile_SelectServiceAsync(botClient, real_chatId, TypeConnect.Free);
                                return;
                            }

                            /////////////// FREE IPSEC ///////////////////////

                            // FREE BUTTON Mobile IPSEC
                            if (button.Data == NamesInlineButtons.Mobile_TryFreePeriod_IpSec)
                            {
                                await mobile_Comands.Mobile_SelectOpSysAsync(botClient, real_chatId, button.Message.Chat?.FirstName, NamesInlineButtons.Mobile_TryFreePeriod_IpSec);
                                return;
                            }

                            // FREE BUTTON Mobile IPSEC IOS
                            if (button.Data == NamesInlineButtons.Mobile_TryFreePeriod_IpSec_ios)
                            {
                                await mobile_Comands.Mobile_BeginFreePeriodAsync(botClient, button, NamesInlineButtons.Mobile_TryFreePeriod_IpSec_ios);
                                return;
                            }

                            // FREE BUTTON Mobile IPSEC ANDROID
                            if (button.Data == NamesInlineButtons.Mobile_TryFreePeriod_IpSec_android)
                            {
                                await mobile_Comands.Mobile_BeginFreePeriodAsync(botClient, button, NamesInlineButtons.Mobile_TryFreePeriod_IpSec_android);
                                return;
                            }

                            ///////////// FREE SOCKS ////////////////////////

                            // FREE BUTTON MOBILE SOCKS
                            if (button.Data == NamesInlineButtons.Mobile_TryFreePeriod_Socks)
                            {
                                await mobile_Comands.Mobile_SelectOpSysAsync(botClient, real_chatId, button.Message.Chat?.FirstName, NamesInlineButtons.Mobile_TryFreePeriod_Socks);
                                return;
                            }

                            // FREE BUTTON MOBILE SOCKS IOS
                            if (button.Data == NamesInlineButtons.Mobile_TryFreePeriod_Socks_ios)
                            {
                                await mobile_Comands.Mobile_BeginFreePeriodAsync(botClient, button, NamesInlineButtons.Mobile_TryFreePeriod_Socks_ios);
                                return;
                            }

                            // FREE BUTTON MOBILE SOCKS ANDROID
                            if (button.Data == NamesInlineButtons.Mobile_TryFreePeriod_Socks_android)
                            {
                                await mobile_Comands.Mobile_BeginFreePeriodAsync(botClient, button, NamesInlineButtons.Mobile_TryFreePeriod_Socks_android);
                                return;
                            }                           

                        }
                        // else { пользователь есть в бд и он неактивный }

                        #region PAYMENT Services

                        /////////////// FREE IPSEC ///////////////////////

                        // PAYMENT BUTTON Mobile IPSEC
                        if (button.Data == NamesInlineButtons.StartIPSEC)
                        {
                            await mobile_Comands.Mobile_SelectOpSysAsync(botClient, real_chatId, button.Message.Chat?.FirstName, NamesInlineButtons.StartIPSEC);
                            return;
                        }

                        // PAYMENT BUTTON Mobile IPSEC IOS
                        if (button.Data == NamesInlineButtons.IPSEC_ios)
                        {
                            await botComands.BotSelectSendInvoiceAsync(botClient, real_chatId, NamesInlineButtons.IPSEC_ios, NamesInlineButtons.StartMobile);
                            return;
                        }

                        // PAYMENT BUTTON Mobile IPSEC ANDROID
                        if (button.Data == NamesInlineButtons.IPSEC_android)
                        {
                            await botComands.BotSelectSendInvoiceAsync(botClient, real_chatId, NamesInlineButtons.IPSEC_android, NamesInlineButtons.StartMobile);
                            return;
                        }

                        ///////////// FREE SOCKS ////////////////////////

                        // PAYMENT BUTTON Mobile SOCKS
                        if (button.Data == NamesInlineButtons.StartSocks)
                        {
                            await mobile_Comands.Mobile_SelectOpSysAsync(botClient, real_chatId, button.Message.Chat?.FirstName, NamesInlineButtons.StartSocks);
                            return;
                        }

                        // PAYMENT BUTTON Mobile SOCKS IOS
                        if (button.Data == NamesInlineButtons.Socks_ios)
                        {
                            await botComands.BotSelectSendInvoiceAsync(botClient, real_chatId, NamesInlineButtons.Socks_ios, NamesInlineButtons.StartMobile);
                            return;
                        }

                        // PAYMENT BUTTON Mobile SOCKS ANDROID
                        if (button.Data == NamesInlineButtons.Socks_android)
                        {
                            await botComands.BotSelectSendInvoiceAsync(botClient, real_chatId, NamesInlineButtons.Socks_android, NamesInlineButtons.StartMobile);
                            return;
                        }

                        #endregion


                        #region Poriod Payment every servers and every services

                        // Если в PAYMENT Services регионе 4 строчки то тут должно быть 4* на количестов пакетов подписки (1мес и 3 мес это 2 пакета) итого 4*2=8
                        
                        //////////////////// 1 MONTH ///////////////////////////////////////////////////////////////////////////////////////////

                        // SELECT PERIOD           1 MONTH IPSEC IOS  
                        if (button.Data == NamesInlineButtons.IPSEC_ios_1_month)
                        {
                            await botComands.BotSendInvoiceAsync(botClient, real_chatId, NamesInlineButtons.IPSEC_ios_1_month, 1, NamesInlineButtons.StartMobile);
                            return;
                        }

                        // SELECT PERIOD           1 MONTH IPSEC ANDROID 
                        if (button.Data == NamesInlineButtons.IPSEC_android_1_month) 
                        {
                            await botComands.BotSendInvoiceAsync(botClient, real_chatId, NamesInlineButtons.IPSEC_android_1_month, 1, NamesInlineButtons.StartMobile);
                            return;
                        }

                        // SELECT PERIOD           1 MONTH SOCKS IOS 
                        if (button.Data == NamesInlineButtons.Socks_ios_1_month)
                        {
                            await botComands.BotSendInvoiceAsync(botClient, real_chatId, NamesInlineButtons.Socks_ios_1_month, 1, NamesInlineButtons.StartMobile);
                            return;
                        }

                        // SELECT PERIOD           1 MONTH SOCKS ANDROID 
                        if (button.Data == NamesInlineButtons.Socks_android_1_month)
                        {
                            await botComands.BotSendInvoiceAsync(botClient, real_chatId, NamesInlineButtons.Socks_android_1_month, 1, NamesInlineButtons.StartMobile);
                            return;
                        }

                        //////////////////// 3 MONTH //////////////////////////////////////////////////////////////////////////////////////////////////////

                        // SELECT PERIOD           3 MONTH IPSEC IOS
                        if (button.Data == NamesInlineButtons.IPSEC_ios_3_month)
                        {
                            await botComands.BotSendInvoiceAsync(botClient, real_chatId, NamesInlineButtons.IPSEC_ios_3_month, 3, NamesInlineButtons.StartMobile);
                            return;
                        }

                        // SELECT PERIOD           3 MONTH IPSEC ANDROID
                        if (button.Data == NamesInlineButtons.IPSEC_android_3_month)
                        {
                            await botComands.BotSendInvoiceAsync(botClient, real_chatId, NamesInlineButtons.IPSEC_android_3_month, 3, NamesInlineButtons.StartMobile);
                            return;
                        }

                        // SELECT PERIOD           3 MONTH SOCKS IOS
                        if (button.Data == NamesInlineButtons.Socks_ios_3_month)
                        {
                            await botComands.BotSendInvoiceAsync(botClient, real_chatId, NamesInlineButtons.Socks_ios_3_month, 3, NamesInlineButtons.StartMobile);
                            return;
                        }

                        // SELECT PERIOD           3 MONTH SOCKS ANDROID
                        if (button.Data == NamesInlineButtons.Socks_android_3_month)
                        {
                            await botComands.BotSendInvoiceAsync(botClient, real_chatId, NamesInlineButtons.Socks_android_3_month, 3, NamesInlineButtons.StartMobile);
                            return;
                        }

                        #endregion
                    }


                    var comp_usr = await botComands.BotCheckUserBDAsync(comp_chatId, 1);  // активный ли пользователь COMP
                     
                    if (comp_usr != null) // АКТИВНЫЙ ЮЗЕР НЕ МОЖЕТ НАЖАТЬ НИКУДА
                    {
                        await botClient.SendTextMessageAsync(real_chatId,
                            $"Ты уже являешься активным пользователем нашего сервиса для ПК 🏆 \n\n" +
                            $"Твоя подписка действует до {comp_usr.DateNextPayment.ToString("dd-MM-yyyy")} 🤝 \n" +
                            $"Ты получишь уведомления от бота за 3,2,1 день до окончания подписки, а также возможность продлить её \n\n" +
                            $"Если возникли какие-либо вопросы, то ты всегда можешь написать в поддержку /support 🥸");
                    }
                    else // неактивный юзер или отсутствует в БД
                    {
                        // START BOT COMP
                        if (button.Data == NamesInlineButtons.StartComp)
                        {
                            await comp_Comands.Comp_BotStartAsync(botClient, real_chatId);
                            return;
                        }

                        var comp_userDB = await botComands.BotCheckUserBDAsync(comp_chatId, 2);  // есть ли ваще в бд

                        if (comp_userDB == null) // если пользователя нет в БД, то он может нажимать на бесплатные кнопки
                        {
                            // START Free period BUTTON COMP
                            if (button.Data == NamesInlineButtons.Comp_TryFreePeriod)
                            {
                                await comp_Comands.Comp_SelectOpSysAsync(botClient, real_chatId, NamesInlineButtons.Comp_TryFreePeriod);
                                return;
                            }

                            // FREE BUTTON Mobile IPSEC MACOS
                            if (button.Data == NamesInlineButtons.Comp_TryFreePeriod_MacOS)
                            {
                                await comp_Comands.Comp_BeginFreePeriodAsync(botClient, button, NamesInlineButtons.Comp_TryFreePeriod_MacOS);
                                return;
                            }

                            // FREE BUTTON Mobile IPSEC WINDOWS
                            if (button.Data == NamesInlineButtons.Comp_TryFreePeriod_Windows)
                            {
                                await comp_Comands.Comp_BeginFreePeriodAsync(botClient, button, NamesInlineButtons.Comp_TryFreePeriod_Windows);
                                return;
                            }
                        }
                        // else { пользователь есть в бд и он неактивный }

                        /////////////////////// PAYMENT Comp ////////////////////////////////////////

                        // PAYMENT BUTTON COMP IPSEC MacOS
                        if (button.Data == NamesInlineButtons.Comp_Payment_MacOS)
                        {
                            await botComands.BotSelectSendInvoiceAsync(botClient, real_chatId, NamesInlineButtons.Comp_Payment_MacOS, NamesInlineButtons.StartComp);
                            return;
                        }

                        // PAYMENT BUTTON COMP IPSEC Windows
                        if (button.Data == NamesInlineButtons.Comp_Payment_Windows)
                        {
                            await botComands.BotSelectSendInvoiceAsync(botClient, real_chatId, NamesInlineButtons.Comp_Payment_Windows, NamesInlineButtons.StartComp);
                            return;
                        }

                        // SELECT PERIOD           1 MONTH IPSEC MacOS  
                        if (button.Data == NamesInlineButtons.Comp_Payment_MacOS_1_month)
                        {
                            await botComands.BotSendInvoiceAsync(botClient, real_chatId, NamesInlineButtons.Comp_Payment_MacOS_1_month, 21, NamesInlineButtons.StartComp);
                            return;
                        }

                        // SELECT PERIOD           3 MONTH IPSEC MacOS  
                        if (button.Data == NamesInlineButtons.Comp_Payment_MacOS_3_month)
                        {
                            await botComands.BotSendInvoiceAsync(botClient, real_chatId, NamesInlineButtons.Comp_Payment_MacOS_3_month, 23, NamesInlineButtons.StartComp);
                            return;
                        }

                        // SELECT PERIOD           1 MONTH IPSEC Windows  
                        if (button.Data == NamesInlineButtons.Comp_Payment_Windows_1_month)
                        {
                            await botComands.BotSendInvoiceAsync(botClient, real_chatId, NamesInlineButtons.Comp_Payment_Windows_1_month, 21, NamesInlineButtons.StartComp);
                            return;
                        }

                        // SELECT PERIOD           3 MONTH IPSEC Windows  
                        if (button.Data == NamesInlineButtons.Comp_Payment_Windows_3_month)
                        {
                            await botComands.BotSendInvoiceAsync(botClient, real_chatId, NamesInlineButtons.Comp_Payment_Windows_3_month, 23, NamesInlineButtons.StartComp);
                            return;
                        }
                    }

                    

                    await botClient.SendTextMessageAsync(real_chatId, "Эта кнопка устарела! 😢\n" +
                        "Выберите /start в меню для начала работы с ботом");

                }
            }
            catch (Exception ex)
            {
                if(update.Message != null)
                    await botClient.SendTextMessageAsync(update.Message.Chat, "Что-то пошло не так, 🤔 обратитесь в поддержку /support");
                Log.Error("Ошибка в HandleUpdateAsync {ex} для пользователя: {chatid}", ex, update?.Message?.Chat.Id);
                return;
            }
        }

        private static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Log.Information("Ошибка в HandleErrorAsync {ex}", ErrorMessage);

            return Task.CompletedTask;
        }

        private static async Task<bool> IsUserMemberOfChannel(ITelegramBotClient botClient, long userId, long channelID)
        {
            try
            {
                var chatMember = await botClient.GetChatMemberAsync(channelID, userId);
                return chatMember.Status == Telegram.Bot.Types.Enums.ChatMemberStatus.Member ||
                       chatMember.Status == Telegram.Bot.Types.Enums.ChatMemberStatus.Administrator ||
                       chatMember.Status == Telegram.Bot.Types.Enums.ChatMemberStatus.Creator;
            }
            catch (Exception ex)
            {
                Log.Information($"Ошибка при проверке подписки: {ex.Message}");
                return false;
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.Information("Запущен бот {botName}", bot.GetMeAsync().Result.FirstName);

            var cancellationToken = new CancellationTokenSource().Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }, // receive all update types
            };

            try
            {
                await bot.ReceiveAsync(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken
                );
            }
            catch (Exception ex)
            {
                // Отправляю себе
                Log.Fatal("Ошибка в TgBotHostedService {errorMassage}", ex.Message);

                await bot.SendTextMessageAsync(1278048494, "Бот вылетел!!! Чини быстрее.\n" +
                    $"Ошибка: {ex.Message}");
                return;
            }
        }
    }
}
