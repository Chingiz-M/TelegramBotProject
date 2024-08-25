using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.InputFiles;
using TelegramBotProject.Intarfaces;

namespace TelegramBotProject.BaseClasses
{
    internal class IPSec : IIPsec1
    {
        public string ServerIPSec { get; set; }
        string PasswordIPSec { get; } = StartUp.GetTokenfromConfig("PasswordAPI");
        /// <summary>
        /// Имя для идентификации inline кнопок бота
        /// </summary>
        public string IPSecName { get; } = "IPSec"; // значение не меняется для определения inline кнопок и платежей
        /// <summary>
        /// значение для определенного сервера и меняется в каждом наследнике и заносится в БД для идентификации конкретного класса
        /// </summary>
        public string NameCertainIPSec { get; set; } = "IPSEC_0";

        private readonly ILogger<IPSec> logger;

        public IPSec(ILogger<IPSec> logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Удаление клиента из ipsec по chatid
        /// </summary>
        /// <param name="clientID"> chatid клиента </param>
        /// <returns> возвращение результата удаления клиента с сервера</returns>
        public async Task<string> DeleteUserAsync(long clientID)
        {
            string result = string.Empty;
            using (var httpClientHandler = new HttpClientHandler())
            {
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                using (var client = new HttpClient(httpClientHandler))
                {
                    string str = $"{ServerIPSec}/deleteconf={clientID}&pass={PasswordIPSec}";
                    var response = await client.GetStringAsync(str);
                    var res = JObject.Parse(response);
                    result = res["result"].ToString();
                    logger.LogInformation("Конфиг пользователя c chatid: {chatid} удален с сервера IPSec", clientID);
                }
            }
            return result;
        }

        /// <summary>
        /// Отзыв конфига клиента из ipsec по chatid
        /// </summary>
        /// <param name="clientID"> chatid клиента </param>
        /// <returns> возвращение результата отзыва конфига клиента с сервера</returns>
        public async Task<string> RevokeUserAsync(long clientID)
        {
            string result = string.Empty;
            using (var httpClientHandler = new HttpClientHandler())
            {
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                using (var client = new HttpClient(httpClientHandler))
                {
                    string str = $"{ServerIPSec}/revokeconf={clientID}&pass={PasswordIPSec}";
                    var response = await client.GetStringAsync(str);
                    var res = JObject.Parse(response);
                    result = res["result"].ToString();
                    logger.LogInformation("Конфиг пользователя c chatid: {chatid} отозван (revoke) с сервера IPSec", clientID);
                }
            }
            return result;
        }
        /// <summary>
        /// Вывод числа активных пользователей ipsec
        /// </summary>
        /// <returns></returns>
        public async Task<int> GetTotalUserAsync()
        {
            int total_users = 0;
            using (var httpClientHandler = new HttpClientHandler())
            {
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                using (var client = new HttpClient(httpClientHandler))
                {
                    string str = $"{ServerIPSec}/getlistclients";
                    var response = await client.GetStringAsync(str);
                    var res = JObject.Parse(response);
                    int.TryParse(res["total_clients"].ToString(), out total_users);
                    logger.LogInformation("Получение всех пользователей IPSec");
                }
            }
            return total_users;
        }
        /// <summary>
        /// Метод создания и отправки конфига пользователю
        /// </summary>
        /// <param name="botClient"> бот клиент </param>
        /// <param name="chatid"> айди нового пользователя для создания имени конфига</param>
        /// <param name="funcname"> название функции апи для выбора формирования конфига на андроид или айос</param>
        /// <param name="fileextension"> расширение конфига для айоса или андроида</param>
        /// <returns></returns>
        // https://176.124.200.227:8433/getconf=55555555&pass=mhR2IFLmm2F3
        public async Task CreateUserConfigAsync(ITelegramBotClient botClient, long chatid, string funcname, string fileextension)
        {
            using (var httpClientHandler = new HttpClientHandler())
            {
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                using (var client = new HttpClient(httpClientHandler))
                {
                    string str = $"{ServerIPSec}/{funcname}={chatid}&pass={PasswordIPSec}";
                    using var s = await client.GetStreamAsync(str);
                    await botClient.SendDocumentAsync(
                        chatId: chatid,
                        document: new InputOnlineFile(content: s, fileName: $"vpnClient{chatid}.{fileextension}"));
                    logger.LogInformation("Создан конфиг пользователя IPSeq chatid: {chatid}", chatid);

                    await botClient.SendTextMessageAsync(1278048494, $"Добавлен пользователь {fileextension} IPSec: {chatid}"); // присылаю себе ответ по добавлению
                    //if (chatid != 1278048494)
                    //    await botClient.SendTextMessageAsync(chatid, $"Добавлен пользователь {fileextension} IPSec: {chatid}"); // присылаю ответ по добавлению

                }
            }
        }
    }
}
