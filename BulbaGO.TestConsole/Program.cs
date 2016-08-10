using System;
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

            Task.Run(async () =>
            {
                var bot = await Bot.CreateNewBot(BotType.NecroBot, AuthType.Ptc, "TBulbaDB005", "qq12534", "US");
                await bot.Start();
            }).Wait();


            Console.WriteLine("Finished, press any key to exit");
            Console.ReadKey();
            //}).Wait();
        }
    }
}
