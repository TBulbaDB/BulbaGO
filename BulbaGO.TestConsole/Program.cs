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
            Log4NetHelper.AddAppender(Log4NetHelper.FileAppender(Level.All));
            ApplicationContext.Initialize();

            //InventoryDownloader.UpdateInventory("TBulbaDB001");

            Task.Run(async () =>
            {
                await MainAsync();
            }).Wait();

            Console.WriteLine("Finished, press any key to exit");
            Console.ReadKey();
            //}).Wait();
        }

        private static async Task MainAsync()
        {
         var bots=new List<Bot>();
            bots.Add(await Bot.GetInstance(AuthType.Ptc, "ceciliakea637271", "xfkc7vpu!", "US"));
            bots.Add(await Bot.GetInstance(AuthType.Ptc, "vickeyshul420902", "hnzgl4iz!", "US"));
            bots.Add(await Bot.GetInstance(AuthType.Ptc, "trevanince766390", "37rh6quj!", "US"));
            bots.Add(await Bot.GetInstance(AuthType.Ptc, "parismchug190803", "q77x8s7z!", "US"));
            bots.Add(await Bot.GetInstance(AuthType.Ptc, "mellissane373075", "k26er6c7!", "US"));
            bots.Add(await Bot.GetInstance(AuthType.Ptc, "edgardobyk937315", "70vknjv0!", "US"));
            bots.Add(await Bot.GetInstance(AuthType.Ptc, "clintonkor908603", "wn4kozq5!", "US"));
            bots.Add(await Bot.GetInstance(AuthType.Ptc, "jeffjosias921058", "re8styl5!", "US"));
            bots.Add(await Bot.GetInstance(AuthType.Ptc, "phebewry46460765", "tua5k806!", "US"));
            bots.Add(await Bot.GetInstance(AuthType.Ptc, "marissasar964321", "4ykaoipy!", "US"));
            bots.Add(await Bot.GetInstance(AuthType.Ptc, "dennywolbe760308", "w2976827!", "AU"));
            bots.Add(await Bot.GetInstance(AuthType.Ptc, "coralieesh717483", "5fqdqpw1!", "US"));
            bots.Add(await Bot.GetInstance(AuthType.Ptc, "assuntadit751889", "6v3068sr!", "AU"));
            bots.Add(await Bot.GetInstance(AuthType.Ptc, "marionstra543463", "x486mq77!", "US"));
            bots.Add(await Bot.GetInstance(AuthType.Ptc, "hildarosel297039", "jp8uwone!", "AU"));
            bots.Add(await Bot.GetInstance(AuthType.Ptc, "delorispas512237", "81zwi5ae!", "US"));
            bots.Add(await Bot.GetInstance(AuthType.Ptc, "yonghartse945354", "471qib82!", "AU"));

            //bots.Add(await Bot.GetInstance(AuthType.Ptc, "TBulbaDB005", "qq12534", "US"));
            Task.WaitAll(bots.Select(b => b.Start(BotType.PokeMobBot)).ToArray());
        }
    }
}
