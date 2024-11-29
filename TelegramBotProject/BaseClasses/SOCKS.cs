using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot;
using TelegramBotProject.Entities;
using TelegramBotProject.Intarfaces;

namespace TelegramBotProject.BaseClasses
{
    internal class SOCKS : ISocks1
    {
        private readonly ILogger<SOCKS> logger;
        public string ServerSocks { get; set; }
        string PasswordSocks { get; } = StartUp.GetTokenfromConfig("PasswordAPI");
        /// <summary>
        /// Имя для идентификации inline кнопок бота
        /// </summary>
        public string SocksName { get; } = "Socks";
        public string NameCertainSocks { get; set; } = "SOCKS_0";

        public SOCKS(ILogger<SOCKS> logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Добавление клиента в носки по chatid
        /// </summary>
        /// <param name="chatid"> chatid клиента использую как имя ключа</param>
        /// <returns> id созданного ключа сопостовимо с именем ключа</returns>
        public async Task<long> CreateUserAsync(ITelegramBotClient botClient, long chatid)
        {
            long id_client = -1;
             string key_client = null;

            try
            {
                using (var httpClientHandler = new HttpClientHandler())
                {
                    httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                    using (var client = new HttpClient(httpClientHandler))
                    {
                        string str = $"{ServerSocks}/createkey={chatid}&pass={PasswordSocks}";
                        var response = await client.GetStringAsync(str);
                        var res = JObject.Parse(response);
                        var mass = res["urlconnection"].ToString();

                        var pattern = @".*key_id='(\d+)'.*access_url='(.*)', used_bytes.*";
                        var match = Regex.Match(mass == null ? "" : mass, pattern);

                        if (match.Success)
                        {
                            long.TryParse(match.Groups[1].ToString(), out id_client); // получаю id ключа из ответа сервера
                            key_client = match.Groups[2].ToString(); // получаю ключ из ответа сервера
                            await botClient.SendTextMessageAsync(chatid, $"Ваш ключ: ");
                            await botClient.SendTextMessageAsync(chatid, $"{key_client}");

                            logger.LogInformation("Создан пользователь Socks, chatid: {chatid}, key_id: {id_client}", chatid, id_client);
                            await botClient.SendTextMessageAsync(1278048494, $"Добавлен пользователь Socks: {chatid}"); // присылаю себе ответ по добавлению
                            //if(chatid != 1278048494)
                            //    await botClient.SendTextMessageAsync(chatid, $"Добавлен пользователь Socks: {chatid}"); // присылаю ответ по добавлению

                        }
                        else
                        {
                            logger.LogError("Ответ с сервера носков не пришел метод CreateUserAsync, chatid: {chatid}", chatid);
                            await botClient.SendTextMessageAsync(1278048494, $"Ответ с сервера носков не пришел метод CreateUserAsync"); // присылаю себе ответ по добавлению
                            //if (chatid != 1278048494)
                            //    await botClient.SendTextMessageAsync(chatid, $"Ответ с сервера носков не пришел метод CreateUserAsync"); // присылаю себе ответ по добавлению
                        }

                    }
                }
                return id_client;
            }
            catch (Exception ex)
            {
                logger.LogError("Ошибка в CreateUserAsync {ex} для пользователя: {chatid}", ex, chatid);
                await botClient.SendTextMessageAsync(chatid, "Что-то пошло не так, 🤔 обратитесь в поддержку /support");

                return id_client;
            }
        }
        /// <summary>
        /// Удаление клиента из носков по chatid
        /// </summary>
        /// <param name="clientkey"> key номер ключа клиента смотри БД</param>
        /// <returns> возвращение результата удаления клиента с сервера</returns>
        public async Task<string> DeleteUserAsync(long clientkey)
        {
            string result = string.Empty;
            using (var httpClientHandler = new HttpClientHandler())
            {
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                using (var client = new HttpClient(httpClientHandler))
                {
                    string str = $"{ServerSocks}/deletedkey={clientkey}&pass={PasswordSocks}";
                    var response = await client.GetStringAsync(str);
                    var res = JObject.Parse(response);
                    result = res["result"].ToString();
                }
            }
            return result;
        }
        /// <summary>
        /// метод получения файла пользователей носков
        /// </summary>
        /// <param name="filename"> имя файла который создается на сервере</param>
        /// <returns> файл csv с пользователями носков </returns>
        public async Task<int> GetFileUsersAsync(string filename, bool createfile)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // для windows-1251
            ServerTotalUsersResponseSocks server_response = new ServerTotalUsersResponseSocks();

            using (var httpClientHandler = new HttpClientHandler())
            {
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                using (var client = new HttpClient(httpClientHandler))
                {
                    string str = $"{ServerSocks}/showallkeys&pass={PasswordSocks}";
                    var response = await client.GetStringAsync(str);
                    server_response = JsonConvert.DeserializeObject<ServerTotalUsersResponseSocks>(response);
                }
            }

            if (createfile)
            {
                var columns = typeof(ServerDataSocks).GetProperties()
                    .Select(property => property.Name).ToList();

                using (StreamWriter sw = new StreamWriter($@"../{filename}", false, Encoding.GetEncoding("windows-1251")))
                {
                    await sw.WriteAsync("Count");
                    await sw.WriteAsync(";");

                    await sw.WriteAsync("keyid");
                    await sw.WriteAsync(";");

                    foreach (var col in columns)
                    {
                        await sw.WriteAsync(col);
                        await sw.WriteAsync(";");
                    }

                    await sw.WriteLineAsync();

                    int count = 1;
                    foreach (var val in server_response.keys)
                    {
                        await sw.WriteAsync($"{count};{val.Key};{val.Value.ToString()}");
                        await sw.WriteLineAsync();
                        count++;
                    }
                }
            }
            
            return server_response.keys.Count();
        }
        /// <summary>
        /// Установка лимита трафика всем клиентам носков
        /// </summary>
        /// <param name="volume_traffic_gb"> объем трафика для ограничения в ГБ</param>
        /// <returns> результат установки трафика </returns>
        public async Task<string> SetLimitTrafficAsync(long volume_traffic_gb)
        {
            string result = string.Empty;
            long voluve_traffic_byte = volume_traffic_gb * 1024 * 1024 * 1024; // перевожу гб в байты
            using (var httpClientHandler = new HttpClientHandler())
            {
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                using (var client = new HttpClient(httpClientHandler))
                {
                    string str = $"{ServerSocks}/setallkeylimitdata={voluve_traffic_byte}&pass={PasswordSocks}";
                    var response = await client.GetStringAsync(str);
                    var res = JObject.Parse(response);
                    result = res["result"].ToString();
                }
            }
            return result;
        }
        /// <summary>
        /// Удаление лимита трафика всем пользователям носков
        /// </summary>
        /// <returns> результат удаления трафика </returns>
        public async Task<string> DeleteLimitTrafficAsync()
        {
            string result = string.Empty;

            using (var httpClientHandler = new HttpClientHandler())
            {
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                using (var client = new HttpClient(httpClientHandler))
                {
                    string str = $"{ServerSocks}/deletedallkeylimitdata&pass={PasswordSocks}";
                    var response = await client.GetStringAsync(str);
                    var res = JObject.Parse(response);
                    result = res["result"].ToString();
                }
            }
            return result;
        }
    }
}
