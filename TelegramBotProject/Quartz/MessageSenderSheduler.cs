using Quartz;
using Quartz.Impl;
using Quartz.Spi;

namespace TelegramBotProject.Quartz
{
    /// <summary>
    /// Класс не используется, остался на всякий случай
    /// </summary>
    public class MessageSenderSheduler
    {
        private readonly IServiceProvider provider;

        public MessageSenderSheduler(IServiceProvider provider)
        {
            this.provider = provider;
        }

        public static async void Start()
        {
            IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            //IJobFactory jobFactory = new JobMessageFactory(provider);
            //scheduler.JobFactory = jobFactory;
            await scheduler.Start();

            IJobDetail job = JobBuilder.Create<MessageSenderJob>().Build();

            ITrigger trigger = TriggerBuilder.Create()  // создаем триггер
                .WithIdentity("trigger1", "group1")     // идентифицируем триггер с именем и группой
                .StartNow()                            // запуск сразу после начала выполнения
                .WithSimpleSchedule(x => x            // настраиваем выполнение действия
                    .WithIntervalInSeconds(200)          // через 200 sec
                    .RepeatForever())                   // бесконечное повторение
                .Build();                               // создаем триггер

            await scheduler.ScheduleJob(job, trigger);        // начинаем выполнение работы
        }
    }
}
