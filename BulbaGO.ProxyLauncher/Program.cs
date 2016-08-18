using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BulbaGO.Base.Bots;
using BulbaGO.Base.Context;
using BulbaGO.Base.Logging;
using BulbaGO.Base.ProcessManagement;
using log4net.Core;

namespace BulbaGO.ProxyLauncher
{
    class Program
    {
        private static SocksWebProxyProcess[] _proxies = new SocksWebProxyProcess[200];
        private static readonly Mutex Mutex = new Mutex(true, "BulbaGO.TestConsole");
        private static CancellationToken ct;

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

            for (int i = 0; i < 100; i++)
            {
                CreateInstance(i);
            }
            var cts = new CancellationTokenSource();
            var ct = cts.Token;
            ct.ThrowIfCancellationRequested();
            Task.WaitAll(_proxies.Where(p => p != null).Select(p => p.Start(cts.Token)).ToArray());
            Console.ReadKey();

        }

        private static async void Proxy_ProcessExited(AsyncProcess process)
        {
            var proxyProcess = process as SocksWebProxyProcess;
            if (proxyProcess == null) return;
            var portBase = proxyProcess.SocksPort - 9001;
            proxyProcess.Logger.Warn("Restarting in 5 seconds.");
            await proxyProcess.Stop();
            await Task.Delay(5000);
            CreateInstance(portBase);
            await _proxies[portBase].Start(ct);
        }

        private static void Proxy_ProcessStateChanged(AsyncProcess process, ProcessState previousState, ProcessState state)
        {
            var proxyProcess = process as SocksWebProxyProcess;
            if (proxyProcess == null) return;
            if (state == ProcessState.Running)
            {
                proxyProcess.Logger.Info($"Connected with exit address {proxyProcess.ExitAddress}");
            }
        }

        private static void CreateInstance(int portBase)
        {
            var proxy = SocksWebProxyProcess.GetInstance(portBase.ToString(), "US", 9001 + portBase);
            proxy.ProcessStateChanged += Proxy_ProcessStateChanged;
            proxy.ProcessExited += Proxy_ProcessExited;
            _proxies[portBase] = proxy;
        }
    }
}
