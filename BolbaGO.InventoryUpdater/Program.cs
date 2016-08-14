using System;
using System.Threading;
using System.Threading.Tasks;
using BulbaGO.Base.BotDataManagement;
using BulbaGO.Base.Context;
using BulbaGO.Base.Logging;
using log4net.Core;

namespace BulbaGO.InventoryUpdater
{
    class Program
    {
        private static readonly Mutex Mutex = new Mutex(true, "BulbaGO.InventoryUpdater");
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

            Task.Run(async () =>
            {
                //var updateInventoryTask = BotDataUpdater.UpdateInventory("TBulbaDB003");
                //updateInventoryTask.Wait(30000);

                if (await BotDataUpdater.UpdateInventory("TBulbaDB001"))
                {
                    BotDataUpdater.Logger.Info("Waiting 30 seconds");
                    await Task.Delay(TimeSpan.FromSeconds(30));
                }

                if (await BotDataUpdater.UpdateInventory("TBulbaDB002"))
                {
                    BotDataUpdater.Logger.Info("Waiting 30 seconds");
                    await Task.Delay(TimeSpan.FromSeconds(30));
                }
                if (await BotDataUpdater.UpdateInventory("TBulbaDB004"))
                {
                    BotDataUpdater.Logger.Info("Waiting 30 seconds");
                    await Task.Delay(TimeSpan.FromSeconds(30));
                }
                if (await BotDataUpdater.UpdateInventory("TBulbaDB005"))
                {
                    BotDataUpdater.Logger.Info("Waiting 30 seconds");
                    await Task.Delay(TimeSpan.FromSeconds(30));
                }

            }).Wait();






            Console.WriteLine("Finished, press any key to exit");
            Console.ReadKey();
        }
    }
}
