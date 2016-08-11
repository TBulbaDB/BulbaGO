using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BulbaGO.Base.Bots;
using BulbaGO.Base.Context;
using BulbaGO.Base.GeoLocation;
using BulbaGO.Base.Logging;
using BulbaGO.Base.SocksProxy;
using log4net.Core;

namespace BulbaGO.TestConsole
{
    class Program
    {
        private static readonly Mutex Mutex = new Mutex(true, "BulbaGO.TestConsole");

        static void Main(string[] args)
        {
            if (!Mutex.WaitOne(TimeSpan.Zero, true))
            {
                Console.WriteLine("There is already an instance of the application is running.");
                Console.ReadKey();
                return;
            }
            Log4NetHelper.AddAppender(Log4NetHelper.ConsoleAppender(Level.All));
            ApplicationContext.Initialize();

            var bots = new List<Bot>();
            //Task.Run(async () =>
            //{
            //    bots.Add(await Bot.CreateNewBot(BotType.NecroBot, AuthType.Ptc, "TBulbaDB001", "qq12534", "US"));
            //    bots.Add(await Bot.CreateNewBot(BotType.NecroBot, AuthType.Ptc, "TBulbaDB002", "qq12534", "US"));
            //}).Wait();


            var taskList = new[]
            {
                Bot.GetInstance(AuthType.Ptc, "TBulbaDB001", "qq12534", "US"),
                Bot.GetInstance(AuthType.Ptc, "TBulbaDB002", "qq12534", "US")
            };
            Task.WaitAll(taskList);

            bots = taskList.Select(t => t.Result).ToList();
            var botTasks = bots.Select(b => b.Start(BotType.NecroBot)).ToArray();
            Task.WaitAll(botTasks);


            Console.WriteLine("Finished, press any key to exit");
            Console.ReadKey();
            //}).Wait();
        }
    }
}
