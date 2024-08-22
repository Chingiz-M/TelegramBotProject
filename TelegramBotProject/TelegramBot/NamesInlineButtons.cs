namespace TelegramBotProject.TelegramBot
{
    internal class NamesInlineButtons
    {
        /// <summary>
        /// переменная для обозначения продления оплаты действующему пользователю
        /// </summary>
        static public string ContinuePayment { get; } = "continue_pay";
        /// <summary>
        /// переменная для пробного периода новому пользователю
        /// </summary>
        static public string TryFreePeriod { get; } = "TryFreePeriod";

        static public string AllUsers { get; } = $"AllUsers";
        static public string IPSEC { get; } = $"IPSec";
        static public string Socks { get; } = $"Socks";

        static public string IOS { get; } = $"ios";
        static public string Android { get; } = $"android";

        static public string Month_1 { get; } = $"1_month";
        static public string Month_3 { get; } = $"3_month";

        static public string StartIPSEC { get; } = $"Button_{IPSEC}";            // Button_IPSec
        static public string StartSocks { get; } = $"Button_{Socks}";            // Button_Socks

        static public string IPSEC_ios { get; } = $"{StartIPSEC}_{IOS}";         // Button_IPSec_ios
        static public string IPSEC_android { get; } = $"{StartIPSEC}_{Android}"; // Button_IPSec_android

        static public string Socks_ios { get; } = $"{StartSocks}_{IOS}";         // Button_Socks_ios
        static public string Socks_android { get; } = $"{StartSocks}_{Android}"; // Button_Socks_android

        static public string ContinuePayment_1_month { get; } = $"{ContinuePayment}_{Month_1}"; // continue_pay_1_month
        static public string ContinuePayment_3_month { get; } = $"{ContinuePayment}_{Month_3}"; // continue_pay_3_month


        static public string IPSEC_ios_1_month { get; } = $"{IPSEC_ios}_{Month_1}";         // Button_IPSec_ios_1_month
        static public string IPSEC_ios_3_month { get; } = $"{IPSEC_ios}_{Month_3}";         // Button_IPSec_ios_3_month
        static public string IPSEC_android_1_month { get; } = $"{IPSEC_android}_{Month_1}"; // Button_IPSec_android_1_month
        static public string IPSEC_android_3_month { get; } = $"{IPSEC_android}_{Month_3}"; // Button_IPSec_android_3_month

        static public string Socks_ios_1_month { get; } = $"{Socks_ios}_{Month_1}";         // Button_Socks_ios_1_month
        static public string Socks_ios_3_month { get; } = $"{Socks_ios}_{Month_3}";         // Button_Socks_ios_3_month
        static public string Socks_android_1_month { get; } = $"{Socks_android}_{Month_1}"; // Button_Socks_android_1_month
        static public string Socks_android_3_month { get; } = $"{Socks_android}_{Month_3}"; // Button_Socks_android_3_month


        #region FreePeriod
        static public string TryFreePeriod_IpSec { get; } = $"{TryFreePeriod}_{StartIPSEC}"; // TryFreePeriod_Button_IPSec
        static public string TryFreePeriod_Socks { get; } = $"{TryFreePeriod}_{StartSocks}"; // TryFreePeriod_Button_Socks


        static public string TryFreePeriod_IpSec_ios { get; } = $"{TryFreePeriod_IpSec}_{IOS}";         // TryFreePeriod_Button_IPSec_ios
        static public string TryFreePeriod_IpSec_android { get; } = $"{TryFreePeriod_IpSec}_{Android}"; // TryFreePeriod_Button_IPSec_android


        static public string TryFreePeriod_Socks_ios { get; } = $"{TryFreePeriod_Socks}_{IOS}";         // TryFreePeriod_Button_Socks_ios
        static public string TryFreePeriod_Socks_android { get; } = $"{TryFreePeriod_Socks}_{Android}"; // TryFreePeriod_Button_Socks_android

        #endregion

    }
}
