using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
            Log4NetHelper.AddAppender(Log4NetHelper.ConsoleAppender(Level.Info));
            ApplicationContext.Initialize();

            foreach (var regionInfo in Countries.All)
            {
                Console.WriteLine("{0} [{1}]", regionInfo.EnglishName, regionInfo.TwoLetterISORegionName);
            }
            //{
            var socksProxyContainers = new List<SocksWebProxyContainer>();
            for (var i = 9001; i <= 9010; i++)
            {
                socksProxyContainers.Add(SocksWebProxyContainer.GetSocksWebProxy(i));
            }
            var startJobs = socksProxyContainers.Select(s => s.Start()).ToArray();
            Task.WaitAll(startJobs);


            Console.ReadKey();
            //}).Wait();
        }
    }
}
