using System;
using System.Collections.Generic;
using BulbaGO.Base.Context;
using BulbaGO.Base.Logging;
using BulbaGO.Base.SocksProxy;
using log4net.Core;

namespace BulbaGO.TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Log4NetHelper.AddAppender(Log4NetHelper.ConsoleAppender(Level.Info));
            ApplicationContext.Initialize();

            //Task.Run(async () =>
            //{
            var socksProxyContainers = new List<SocksWebProxyContainer>();
            for (var i = 9001; i <= 9010; i++)
            {
                socksProxyContainers.Add(SocksWebProxyContainer.GetSocksWebProxy(i));
            }
            socksProxyContainers.ForEach(p => p.Start());
            //var startJobs = socksProxyContainers.Select(s => s.Start()).ToArray();
            //Task.WaitAll(startJobs);


            Console.ReadKey();
            //}).Wait();
        }
    }
}
