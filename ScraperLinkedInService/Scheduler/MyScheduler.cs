using System;

namespace ScraperLinkedInService.Scheduler
{
    public static class MyScheduler
    {
        public static void IntervalInHours(int hour, int min, double interval, Action task)
        {
            SchedulerService.Instance.ScheduleTask(hour, min, interval, task);
        }

        public static void IntervalInDays(int hour, int min, double interval, Action task)
        {
            interval = interval * 24;
            SchedulerService.Instance.ScheduleTask(hour, min, interval, task);
        }

        public static void Clear()
        {
            SchedulerService.Instance.Clear();
        }
    }
}
