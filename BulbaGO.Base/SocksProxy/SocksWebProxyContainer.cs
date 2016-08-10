using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BulbaGO.Base.GeoLocation;
using BulbaGO.Base.HttpUtils;
using BulbaGO.Base.ProcessHandling;
using com.LandonKey.SocksWebProxy;
using com.LandonKey.SocksWebProxy.Proxy;
using log4net;
using MaxMind.GeoIP2.Model;

namespace BulbaGO.Base.SocksProxy
{
    public class SocksWebProxyContainer : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(SocksWebProxyContainer));
        private static readonly string TorExecutablePath;
        private static readonly string TorCommandLineTemplate;
        private static readonly string TorCommandLineCountriesTemplate;

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
            TorCommandLineCountriesTemplate = " --ExitNodes {0} --StrictNodes 1";
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

        public static SocksWebProxyContainer GetSocksWebProxy(int port, List<string> countries = null)
        {
            return ProxyContainers.GetOrAdd(port, p => new SocksWebProxyContainer { SocksPort = port, TorCountries = countries });
        }

        public string ExitAddress { get; set; }
        public int SocksPort { get; set; }
        public int HttpPort => SocksPort + 1000;
        public SocksWebProxy Proxy { get; set; }
        public Process TorProcess { get; set; }
        public Country Country { get; set; }
        public List<string> TorCountries { get; set; }
        public bool IsRunning { get; set; }

        private bool _suppressErrors;

        public async Task Start()
        {
            Logger.Info($"Launching Tor on Port {SocksPort}");
            await LaunchTorInstance();
        }

        private void UpdateProxy()
        {
            var proxyConfig = new ProxyConfig(IPAddress.Loopback, HttpPort, IPAddress.Loopback, SocksPort, ProxyConfig.SocksVersion.Five);
            Proxy = new SocksWebProxy(proxyConfig);
        }

        private async Task UpdateExitAddress()
        {
            ExitAddress = await HttpHelpers.WhatIsMyIp(Proxy);
            if (string.IsNullOrWhiteSpace(ExitAddress))
            {
                Logger.Warn($"Proxy on port {SocksPort} has empty exit address, relaunching tor instance");
                await RelaunchTorInstance();
                return;
            }
            if (ProxyContainers.Any(pc => pc.Value != this && pc.Value.ExitAddress == this.ExitAddress))
            {
                Logger.Warn($"Proxy on port {SocksPort} has duplicate exit address, relaunching tor instance.");
                await RelaunchTorInstance();
                return;
            }

            Country = GeoIpHelper.GetCountry(ExitAddress);
            Logger.Info($"Proxy on port {SocksPort} is now connected to {Country.Name} [{Country.IsoCode}] with ip address {ExitAddress}");
        }

        private async Task RelaunchTorInstance()
        {
            KillTorInstance();
            await LaunchTorInstance();
        }

        private void KillTorInstance()
        {
            if (TorProcess == null) return;
            _suppressErrors = true;
            try
            {
                TorProcess.Kill();
            }
            catch (Exception ex)
            {
                Logger.Error($"An error occured while killing the tor process: ({ex.GetType().Name}) {ex.Message}");
            }
            TorProcess = null;
            IsRunning = false;
        }

        private Task LaunchTorInstance()
        {
            var outputData = new StringBuilder();
            var completed = false;
            var tcs = new TaskCompletionSource<object>();
            KillTorInstance();

            TorProcess = new Process();
            TorProcess.EnableRaisingEvents = true;

            var torArguments = string.Format(TorCommandLineTemplate, SocksPort);

            if (TorCountries != null && TorCountries.Count > 0)
            {
                var countries = "{" + string.Join(",", TorCountries) + "}";
                torArguments += string.Format(TorCommandLineCountriesTemplate, countries);
            }
            TorProcess.StartInfo = new ProcessStartInfo
            {
                FileName = TorExecutablePath,
                Arguments = torArguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            TorProcess.OutputDataReceived += Process_OutputDataReceived;
            TorProcess.OutputDataReceived += (s, ea) =>
            {
                if (completed) return;
                if (ea.Data != null)
                {
                    outputData.Append(ea.Data);
                }
                if (outputData.ToString().Contains("Bootstrapped 100%: Done"))
                {
                    completed = true;
                    Task.Delay(100);
                    tcs.SetResult(null);
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
            if (!_suppressErrors)
            {
                Logger.Error(e.Data);
            }
            else
            {
                _suppressErrors = false;
            }
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Logger.Debug(e.Data);
        }

        public void Dispose()
        {
            KillTorInstance();
        }
    }
}
