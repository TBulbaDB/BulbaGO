using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BulbaGO.Base.Context;
using BulbaGO.Base.GeoIp;
using BulbaGO.Base.HttpUtils;
using BulbaGO.Base.ProcessHandling;
using com.LandonKey.SocksWebProxy;
using com.LandonKey.SocksWebProxy.Proxy;
using log4net;
using MaxMind.GeoIP2.Model;
using MongoDB.Bson.IO;

namespace BulbaGO.Base.SocksProxy
{
    public class SocksWebProxyContainer
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(SocksWebProxyContainer));
        private static readonly string TorExecutablePath;
        private static readonly string TorCommandLineTemplate;

        static SocksWebProxyContainer()
        {
            TorExecutablePath = Path.Combine(Environment.CurrentDirectory, "Tor", "Tor", "tor.exe");
            KillExistingTorProcesses();

            var torDataPath = Path.Combine(Environment.CurrentDirectory, "Tor", "Data");
            var geoipPath = Path.Combine(torDataPath, "Tor", "geoip");
            var geoip6Path = Path.Combine(torDataPath, "Tor", "geoip6");
            TorCommandLineTemplate =
                "--ClientOnly 1 --SocksPort {0} --SocksBindAddress 127.0.0.1 --DataDirectory " + torDataPath +
                "\\{0} --GeoIPFile " + geoipPath + " --GeoIPv6File " + geoip6Path;
            //            "--ClientOnly 1 --SocksPort {0} --SocksBindAddress 127.0.0.1 --DataDirectory " + torDataPath + "\\{0} --GeoIPFile " + geoipPath + " --GeoIPv6File " + geoip6Path + " --ExitNodes {1} --StrictNodes 1";
        }

        private static void KillExistingTorProcesses()
        {
            var processes = Process.GetProcesses();
            foreach (var process in processes)
            {
                if (process.StartInfo.FileName == TorExecutablePath)
                {
                    process.Kill();
                }
            }
        }

        private static readonly ConcurrentDictionary<int, SocksWebProxyContainer> ProxyContainers = new ConcurrentDictionary<int, SocksWebProxyContainer>();

        public static SocksWebProxyContainer GetSocksWebProxy(int port, string countries = "{us}")
        {
            return ProxyContainers.GetOrAdd(port, p => new SocksWebProxyContainer { LocalPort = port });
        }

        public string ExitAddress { get; set; }
        public int LocalPort { get; set; }
        public SocksWebProxy Proxy { get; set; }
        public Process TorProcess { get; set; }
        public Country Country { get; set; }
        public bool IsRunning { get; set; }

        public Task<bool> Start()
        {
            Logger.Info($"Launching Tor on Port {LocalPort}");
            return LaunchTorInstance();
        }

        private void UpdateProxy()
        {
            var proxyConfig = new ProxyConfig(IPAddress.Loopback, LocalPort + 1000, IPAddress.Loopback, LocalPort, ProxyConfig.SocksVersion.Five);
            Proxy = new SocksWebProxy(proxyConfig);
        }

        private async Task UpdateExitAddress()
        {
            ExitAddress = await HttpHelpers.WhatIsMyIp(Proxy);
            if (ProxyContainers.Any(pc => pc.Value.ExitAddress == this.ExitAddress))
            {
                //Logger.Warn("Duplicate exit address detected, relaunching tor instance.");
            }

            Country = GeoIpHelper.GetCountry(ExitAddress);
            Logger.Info($"Proxy on port {LocalPort} is now connected to {Country.Name} with ip address {ExitAddress}");
        }

        private void RelaunchTorInstance()
        {
            
        }

        private Task<bool> LaunchTorInstance()
        {
            var tcs = new TaskCompletionSource<bool>();

            if (TorProcess != null)
            {
                try
                {
                    TorProcess.Kill();
                    TorProcess = null;
                }
                catch (Exception)
                {
                }
            }

            TorProcess = new Process();
            TorProcess.EnableRaisingEvents = true;
            TorProcess.StartInfo = new ProcessStartInfo
            {
                FileName = TorExecutablePath,
                Arguments = string.Format(TorCommandLineTemplate, LocalPort),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            TorProcess.OutputDataReceived += Process_OutputDataReceived;
            TorProcess.OutputDataReceived += (s, ea) =>
            {
                if (ea.Data.Contains("Bootstrapped 100%: Done"))
                {
                    tcs.SetResult(true);
                    IsRunning = true;
                    UpdateProxy();
                    UpdateExitAddress().Wait();

                }
            };

            TorProcess.ErrorDataReceived += Process_ErrorDataReceived;
            TorProcess.Exited += Process_Exited;
            var started = TorProcess.Start();
            if (!started)
            {
                //you may allow for the process to be re-used (started = false) 
                //but I'm not sure about the guarantees of the Exited event in such a case
                throw new InvalidOperationException("Could not start process: " + TorProcess);
            }
            ChildProcessTracker.AddProcess(TorProcess);
            TorProcess.BeginOutputReadLine();
            TorProcess.BeginErrorReadLine();
            return tcs.Task; ;
        }

        private void Process_Exited(object sender, EventArgs e)
        {
            IsRunning = false;
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Logger.Error(e.Data);
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Logger.Debug(e.Data);
        }
    }
}
