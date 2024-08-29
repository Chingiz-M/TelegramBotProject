using System;
using System.Collections.Generic;

namespace TelegramBotProject.Entities;

public class UserDB
{
    public int Id { get; set; }
    public long ChatID { get; set; }
    public string Status { get; set; } = "active";
    public string? FirstName { get; set; }
    public string? Username { get; set; }
    public DateTime DateCreate { get; set; } = DateTime.Now;
    public DateTime? DateDisconnect { get; set; }
    public DateTime DateNextPayment { get; set; } = DateTime.Now.AddDays(14); // по умолчанию месяц пробный период
    public string? ProviderPaymentChargeId { get; set; } // номер транзакции. По нему можно будет найти платёж в личном кабинете.
    public string Role { get; set; } = "user";
    public bool Blatnoi { get; set; } = false;
    public string? Comment { get; set; }
    public int Referal { get; set; } = 0; // кол-во человек который привел пользователь
    public long ReferalVoice { get; set; } // chatid пользователя за которого проголосовал этот пользователь
    public string NameService { get; set; } // Имя сервера ipsec или socks
    public string NameOS { get; set; }// название ОС (ios ir andriod)
    public long ServiceKey { get; set; } // ключ для носков а для ipsec повтор chatid
    public string ServiceAddress { get; set; } // адрес сервака
    public string TypeOfDevice { get; set; } = "mobile"; // вид устройства (mobile, computer)
    public bool UsePromocode { get; set; } = false; // использование промокода
}
