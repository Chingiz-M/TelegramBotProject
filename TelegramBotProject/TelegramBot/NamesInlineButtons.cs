namespace TelegramBotProject.TelegramBot
{
    internal class NamesInlineButtons
    {
        static public string ContinuePayment { get; } = "continue_pay";   // переменная для обозначения продления оплаты
        static public string Month_1 { get; } = $"1_month";
        static public string Month_3 { get; } = $"3_month";


        #region Mobile

        static public string StartMobile { get; } = "Mobile_config";        // переменная для выбора конфига для мобильных устройств

        static public string ContinuePayment_Mobile { get; } = $"{ContinuePayment}_{StartMobile}";   // переменная для обозначения продления оплаты действующему пользователю моб устройства
        static public string ContinuePayment_Mobile_1_month { get; } = $"{ContinuePayment_Mobile}_{Month_1}"; // continue_pay_mobile_1_month
        static public string ContinuePayment_Mobile_3_month { get; } = $"{ContinuePayment_Mobile}_{Month_3}"; // continue_pay_mobile_3_month

        static public string Mobile_TryFreePeriod { get; } = "Mobile_TryFreePeriod"; // переменная для пробного периода новому пользователю

        static public string AllUsers { get; } = $"AllUsers";
        static public string IPSEC { get; } = $"IPSec";
        static public string Socks { get; } = $"Socks";

        static public string IOS { get; } = $"ios";
        static public string Android { get; } = $"android";

        static public string StartIPSEC { get; } = $"Button_{IPSEC}";            // Button_IPSec
        static public string StartSocks { get; } = $"Button_{Socks}";            // Button_Socks

        static public string IPSEC_ios { get; } = $"{StartIPSEC}_{IOS}";         // Button_IPSec_ios
        static public string IPSEC_android { get; } = $"{StartIPSEC}_{Android}"; // Button_IPSec_android

        static public string Socks_ios { get; } = $"{StartSocks}_{IOS}";         // Button_Socks_ios
        static public string Socks_android { get; } = $"{StartSocks}_{Android}"; // Button_Socks_android


        static public string IPSEC_ios_1_month { get; } = $"{IPSEC_ios}_{Month_1}";         // Button_IPSec_ios_1_month
        static public string IPSEC_ios_3_month { get; } = $"{IPSEC_ios}_{Month_3}";         // Button_IPSec_ios_3_month
        static public string IPSEC_android_1_month { get; } = $"{IPSEC_android}_{Month_1}"; // Button_IPSec_android_1_month
        static public string IPSEC_android_3_month { get; } = $"{IPSEC_android}_{Month_3}"; // Button_IPSec_android_3_month

        static public string Socks_ios_1_month { get; } = $"{Socks_ios}_{Month_1}";         // Button_Socks_ios_1_month
        static public string Socks_ios_3_month { get; } = $"{Socks_ios}_{Month_3}";         // Button_Socks_ios_3_month
        static public string Socks_android_1_month { get; } = $"{Socks_android}_{Month_1}"; // Button_Socks_android_1_month
        static public string Socks_android_3_month { get; } = $"{Socks_android}_{Month_3}"; // Button_Socks_android_3_month


        #region FreePeriod
        static public string Mobile_TryFreePeriod_IpSec { get; } = $"{Mobile_TryFreePeriod}_{StartIPSEC}"; // Mobile_TryFreePeriod_Button_IPSec
        static public string Mobile_TryFreePeriod_Socks { get; } = $"{Mobile_TryFreePeriod}_{StartSocks}"; // Mobile_TryFreePeriod_Button_Socks


        static public string Mobile_TryFreePeriod_IpSec_ios { get; } = $"{Mobile_TryFreePeriod_IpSec}_{IOS}";         // Mobile_TryFreePeriod_Button_IPSec_ios
        static public string Mobile_TryFreePeriod_IpSec_android { get; } = $"{Mobile_TryFreePeriod_IpSec}_{Android}"; // Mobile_TryFreePeriod_Button_IPSec_android


        static public string Mobile_TryFreePeriod_Socks_ios { get; } = $"{Mobile_TryFreePeriod_Socks}_{IOS}";         // Mobile_TryFreePeriod_Button_Socks_ios
        static public string Mobile_TryFreePeriod_Socks_android { get; } = $"{Mobile_TryFreePeriod_Socks}_{Android}"; // Mobile_TryFreePeriod_Button_Socks_android

        #endregion

        #endregion

        #region Comp

        static public string StartComp { get; } = "Comp_config";  // переменная для выбора конфига для компа, ноута

        static public string ContinuePayment_Comp { get; } = $"{ContinuePayment}_{StartComp}";   // переменная для обозначения продления оплаты действующему пользователю ПК
        static public string ContinuePayment_Comp_1_month { get; } = $"{ContinuePayment_Comp}_{Month_1}"; // continue_pay_comp_1_month
        static public string ContinuePayment_Comp_3_month { get; } = $"{ContinuePayment_Comp}_{Month_3}"; // continue_pay_comp_3_month

        static public string Windows { get; } = $"Windows";
        static public string MacOS { get; } = $"MacOS";

        static public string Comp_TryFreePeriod { get; } = "Comp_TryFreePeriod";                            // переменная для пробного периода новому пользователю
        static public string Comp_TryFreePeriod_Windows { get; } = $"{Comp_TryFreePeriod}_{Windows}";       // Comp_TryFreePeriod_Windows
        static public string Comp_TryFreePeriod_MacOS { get; } = $"{Comp_TryFreePeriod}_{MacOS}";           // Comp_TryFreePeriod_MacOS

        static public string Comp_Payment { get; } = "Comp_Payment";                                        // переменная для платного периода
        static public string Comp_Payment_Windows { get; } = $"{Comp_Payment}_{Windows}";                   // Comp_Payment_Windows
        static public string Comp_Payment_MacOS { get; } = $"{Comp_Payment}_{MacOS}";                       // Comp_Payment_MacOS

        static public string Comp_Payment_MacOS_1_month { get; } = $"{Comp_Payment_MacOS}_{Month_1}";         // Comp_Payment_MacOS_1_month
        static public string Comp_Payment_MacOS_3_month { get; } = $"{Comp_Payment_MacOS}_{Month_3}";         // Comp_Payment_MacOS_3_month
        static public string Comp_Payment_Windows_1_month { get; } = $"{Comp_Payment_Windows}_{Month_1}";    // Comp_Payment_Windows_1_month
        static public string Comp_Payment_Windows_3_month { get; } = $"{Comp_Payment_Windows}_{Month_3}";    // Comp_Payment_Windows_3_month

        #endregion

    }
}
