using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BulbaGO.Base.Bots;
using BulbaGO.Base.Context;
using BulbaGO.Base.GeoLocation;
using BulbaGO.Base.GeoLocation.Google;
using BulbaGO.Base.Logging;
using BulbaGO.Base.Scheduler;
using BulbaGO.Base.Utils;
using log4net.Core;
using Timer = System.Timers.Timer;

namespace BulbaGO.TestConsole
{
    class Program
    {
        private static readonly Mutex Mutex = new Mutex(true, "BulbaGO.TestConsole");
        private static readonly List<Bot> Bots = new List<Bot>();
        private static readonly Timer HeartbeatTimer = new Timer(1000);


        static void Main(string[] args)
        {
            if (!Mutex.WaitOne(TimeSpan.Zero, true))
            {
                Console.WriteLine("There is already an instance of the application is running.");
                Console.ReadKey();
                return;
            }
            Log4NetHelper.AddAppender(Log4NetHelper.ConsoleAppender(Level.All));
            Log4NetHelper.AddAppender(Log4NetHelper.FileAppender(Level.All));
            ApplicationContext.Initialize();

            //InventoryDownloader.UpdateInventory("TBulbaDB001");

            HeartbeatTimer.Elapsed += HeartbeatTimer_Elapsed;
            Task.Run(async () =>
            {
                await MainAsync();
            }).Wait();

            Console.WriteLine("Finished, press any key to exit");
            Console.ReadKey();
            //}).Wait();
        }

        private static void HeartbeatTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            HeartbeatTimer.Stop();
            Task.Run(async () =>
            {
                var botsToStop = Bots.Where(b => b.Schedule != null && !b.Schedule.IsInScheduledTime()).ToList();
                if (botsToStop.Count == 0) return;
                foreach (var bot in botsToStop)
                {
                    await bot.Stop();
                }
            });
            HeartbeatTimer.Start();
        }

        private static async Task MainAsync()
        {
            //HeartbeatTimer.Start();
            Bots.Add(await Bot.GetInstance(AuthType.Ptc, "trevanince766390", "37rh6quj!", "US"));
            Bots.Add(await Bot.GetInstance(AuthType.Ptc, "dennywolbe760308", "w2976827!", "US"));
            Bots.Add(await Bot.GetInstance(AuthType.Ptc, "coralieesh717483", "5fqdqpw1!", "US"));
            Bots.Add(await Bot.GetInstance(AuthType.Ptc, "assuntadit751889", "6v3068sr!", "US"));
            Bots.Add(await Bot.GetInstance(AuthType.Ptc, "marionstra543463", "x486mq77!", "US"));
            Bots.Add(await Bot.GetInstance(AuthType.Ptc, "hildarosel297039", "jp8uwone!", "US"));
            Bots.Add(await Bot.GetInstance(AuthType.Ptc, "delorispas512237", "81zwi5ae!", "US"));
            Bots.Add(await Bot.GetInstance(AuthType.Ptc, "yonghartse945354", "471qib82!", "US"));
            Bots.Add(await Bot.GetInstance(AuthType.Ptc, "nohemicupp420461", "uooh4iv4!", "US"));
            Task.WaitAll(Bots.Select(b => b.Start(BotType.PokeMobBot)).ToArray());
        }
    }
}
