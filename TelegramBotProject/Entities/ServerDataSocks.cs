//using Telegram.Bots.Types;

namespace TelegramBotProject.Entities
{
    public class ServerDataSocks
    {
        public string name { get; set; }
        private double? used_bytes;
        public double? Used_bytes
        {
            get { return used_bytes; }
            init
            {
                if (value != null)
                    used_bytes = value / 1024 / 1024 / 1024;
                else
                    used_bytes = 0;
            }
        }

        private double? data_limit;
        public double? Data_limit
        {
            get { return data_limit; }
            init
            {
                if (value != null)
                    data_limit = value / 1024 / 1024 / 1024;
                else
                    data_limit = 0;
            }
        }
        public override string? ToString()
        {
            return $"{name};{string.Format("{0:f2}", Used_bytes)};{string.Format("{0:f2}", Data_limit)}";
        }
    }
}