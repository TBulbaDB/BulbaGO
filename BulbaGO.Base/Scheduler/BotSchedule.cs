using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulbaGO.Base.Scheduler
{
    public class BotSchedule
    {
        private static Random _random = new Random();
        public List<TimeRange> Ranges { get; set; } = new List<TimeRange>();

        public bool IsInScheduledTime()
        {
            var dateTime = DateTime.Now;
            return Ranges.Any(r => r.IsWithinRange(dateTime));
        }

        public static BotSchedule Generate()
        {
            return Generate(_random.Next(6, 9), _random.Next(9, 13), _random.Next(3, 5));
        }

        public static BotSchedule Generate(int minHoursPerDay, int maxHoursPerDay, int dailySessionsCount = 3)
        {
            var randomTime = _random.NextDouble() * (maxHoursPerDay - minHoursPerDay) + minHoursPerDay;

            var dailyRunningTime = TimeSpan.FromHours(randomTime);

            if (dailyRunningTime > TimeSpan.FromHours(12))
            {
                throw new ArgumentException("Cannot be bigger than 12 hours", nameof(dailyRunningTime));
            }
            if (dailySessionsCount <= 0) dailySessionsCount = 1;

            var nonRunningTimeInSeconds = (int)(TimeSpan.FromDays(1) - dailyRunningTime).TotalSeconds;

            var result = new BotSchedule();

            if (dailySessionsCount == 1)
            {
                var startTime = TimeSpan.FromSeconds(_random.Next(nonRunningTimeInSeconds));
                var endTime = startTime + dailyRunningTime;
                result.Ranges.Add(new TimeRange(startTime, endTime));
            }
            else
            {
                var totalSessions = dailySessionsCount; // Including the non working times

                var sessionRandoms = new List<double>();
                for (var i = 0; i < totalSessions; i++)
                {
                    sessionRandoms.Add(_random.Next(7000,10000));
                }
                var sessionRandomsSum = sessionRandoms.Sum();
                var sessionsLengths =
                    sessionRandoms.Select(s => TimeSpan.FromSeconds((s / sessionRandomsSum) * dailyRunningTime.TotalSeconds)).ToList();



                var totalIdleSeconds = TimeSpan.FromDays(1).TotalSeconds - dailyRunningTime.TotalSeconds;

                var totalIdleSessions = dailySessionsCount + 1;

                var idleSessionRandoms = new List<double>();
                for (var i = 0; i < totalIdleSessions; i++)
                {
                    idleSessionRandoms.Add(_random.Next(5000,10000));
                }
                var idleSessionRandomsSum = idleSessionRandoms.Sum();
                var idleSessionsLengths =
                    idleSessionRandoms.Select(s => TimeSpan.FromSeconds((s / idleSessionRandomsSum) * totalIdleSeconds)).ToList();




                var baseTime = idleSessionsLengths[0];
                for (var i = 0; i < totalSessions; i++)
                {
                    result.Ranges.Add(new TimeRange(baseTime, baseTime + sessionsLengths[i]));
                    baseTime += sessionsLengths[i] + idleSessionsLengths[i];
                }

            }

            result.Ranges = result.Ranges.Where(r => r.Start <= TimeSpan.FromHours(23.5)).ToList();
            foreach (var timeRange in result.Ranges)
            {
                if (timeRange.End > TimeSpan.FromDays(23.75))
                {
                    timeRange.End = TimeSpan.FromHours(23.75);
                }
            }


            return result;
        }
    }

    public class TimeRange
    {
        public TimeSpan Start { get; set; }
        public TimeSpan End { get; set; }

        public TimeRange() { }

        public TimeRange(TimeSpan start, TimeSpan end)
        {
            Start = start;
            End = end;
        }

        public bool IsWithinRange(DateTime dateTime)
        {
            var timeOfDay = dateTime.TimeOfDay;
            return timeOfDay >= Start && timeOfDay <= End;
        }
    }

}
