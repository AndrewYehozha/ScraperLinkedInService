﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ScraperLinkedInService.Scheduler
{
    public class SchedulerService : IDisposable
    {
        private static SchedulerService _instance;
        private List<Timer> timers;

        private SchedulerService() {
            timers = new List<Timer>();
        }

        public static SchedulerService Instance => _instance ?? (_instance = new SchedulerService());

        public void ScheduleTask(int hour, int min, double intervalInHour, Action task)
        {
            var now = DateTime.Now;
            var firstRun = new DateTime(now.Year, now.Month, now.Day, hour, min, 0, 0);
            if (now > firstRun)
            {
                firstRun = firstRun.AddDays(1);
            }

            TimeSpan timeToGo = firstRun - now;
            if (timeToGo <= TimeSpan.Zero)
            {
                timeToGo = TimeSpan.Zero;
            }

            var timer = new Timer(x =>
            {
                task.Invoke();
            }, null, timeToGo, TimeSpan.FromHours(intervalInHour));

            timers.Add(timer);
        }

        public void Clear()
        {
            timers = null;
        }

        public void Dispose()
        {
            if (timers.Any())
            {
                foreach (var timer in timers)
                {
                    if (timer != null)
                    {
                        timer.Dispose();
                    }
                }
            }
        }
    }
}
