using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BulbaGO.Base.Bots;
using BulbaGO.Base.GeoLocation;
using BulbaGO.Base.SocksProxy.SocksWebProxy;
using BulbaGO.Base.SocksProxy.SocksWebProxy.Proxy;
using BulbaGO.Base.Utils;
using MaxMind.GeoIP2.Model;

namespace BulbaGO.Base.ProcessManagement
{
    public class SocksWebProxyProcess : AsyncProcess
    {
        private static readonly string TorExecutablePath;
        private static readonly string TorCommandLineTemplate;
        private static readonly string TorCommandLineCountriesTemplate;

        //private static readonly string TorBridgesTemplate;
        private static readonly string TorPath;

        public static readonly HashSet<string> BlockedIps = new HashSet<string> { "139.59.9.200" };

        static SocksWebProxyProcess()
        {
            TorPath = Path.Combine(Environment.CurrentDirectory, "Tor", "Tor");
            TorExecutablePath = Path.Combine(Environment.CurrentDirectory, "Tor", "Tor", "tor.exe");
            KillExistingTorProcesses();

            var torDataPath = Path.Combine(Environment.CurrentDirectory, "Tor", "Data");
            var geoipPath = Path.Combine(torDataPath, "Tor", "geoip");
            var geoip6Path = Path.Combine(torDataPath, "Tor", "geoip6");

            TorCommandLineTemplate =
            "--ClientOnly 1 --SocksPort {0} --SocksBindAddress 127.0.0.1 --NewCircuitPeriod 30000000 --AllowUnverifiedNodes middle,rendezvous,exit --DataDirectory " + torDataPath +
            "\\{0} --GeoIPFile " + geoipPath + " --GeoIPv6File " + geoip6Path;
            TorCommandLineCountriesTemplate = " --ExitNodes {0} --StrictNodes 1";
            //TorBridgesTemplate = " -f torbridgeconfig.ini";
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

        private const int MinPort = 9051;
        private const int MaxPort = 10050;
        private static readonly Random PortRandomizer = new Random();
        private static readonly HashSet<int> UsedPorts = new HashSet<int>();
        private static readonly HashSet<string> ExitAddresses = new HashSet<string>();

        public static SocksWebProxyProcess GetInstance(Bot bot, int socksPort = 0)
        {
            return GetInstance(bot, new List<string>(), socksPort);
        }

        public static SocksWebProxyProcess GetInstance(Bot bot, string isoTwoLetterCountryCode, int socksPort = 0)
        {
            return GetInstance(bot, new List<string> { isoTwoLetterCountryCode }, socksPort);
        }

        public static SocksWebProxyProcess GetInstance(Bot bot, List<string> isoTwoLetterCountryCodes, int socksPort = 0)
        {
            int proxyPort;
            if (socksPort == 0)
            {
                proxyPort = PortRandomizer.Next(MinPort, MaxPort);
                while (UsedPorts.Contains(proxyPort) || IpTools.IsPortInUse(proxyPort) ||
                       IpTools.IsPortInUse(proxyPort + 1000))
                {
                    proxyPort = PortRandomizer.Next(MinPort, MaxPort);
                }
                UsedPorts.Add(proxyPort);
            }
            else
            {
                proxyPort = socksPort;
            }
            return new SocksWebProxyProcess(bot) { TorCountries = isoTwoLetterCountryCodes, SocksPort = proxyPort, Timeout = 120000 };
        }

        public static SocksWebProxyProcess GetInstance(string loggerName, string isoTwoLetterCountryCode, int socksPort)
        {
            var fakeBot = new Bot { Username = loggerName };
            return GetInstance(fakeBot, isoTwoLetterCountryCode, socksPort);
        }


        private SocksWebProxyProcess(Bot bot) : base(bot)
        {

        }

        public string ExitAddress { get; set; }
        public int SocksPort { get; set; }
        public int HttpPort => SocksPort + 1000;
        public SocksWebProxy Proxy { get; set; }
        public Process TorProcess { get; set; }
        public Country Country { get; set; }
        public List<string> TorCountries { get; set; }
        public bool Connected { get; set; }
        public bool InUse { get; set; }

        public long Timeout { get; set; }

        private StringBuilder _consoleOutput;
        private Stopwatch _timeoutChecker;
        private static readonly Random BridgeRandomizer = new Random();

        public async Task<bool> Start(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var torArguments = string.Format(TorCommandLineTemplate, SocksPort);

            if (TorCountries != null && TorCountries.Count > 0)
            {
                var countries = "{" + string.Join(",", TorCountries) + "}";
                torArguments += string.Format(TorCommandLineCountriesTemplate, countries);
            }

            //if (BridgeRandomizer.Next(2) == 0)
            //{
            //    torArguments += TorBridgesTemplate;
            //}

            var startInfo = new ProcessStartInfo
            {
                FileName = TorExecutablePath,
                Arguments = torArguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = TorPath
            };
            ProcessExited += SocksWebProxyProcess_ProcessExited;
            //Logger.Debug($"{TorExecutablePath} {torArguments}");

            await Start(startInfo, ct);
            if (State != ProcessState.Started)
            {
                State = ProcessState.StartFailed;
                return false;
            }

            _consoleOutput = new StringBuilder();

            State = ProcessState.Initializing;
            if (await Initialize(ct))
            {
                State = ProcessState.Running;
                return true;
            }
            State = ProcessState.InitializationFailed;
            return false;
        }

        private void SocksWebProxyProcess_ProcessExited(AsyncProcess process)
        {
            if (!string.IsNullOrWhiteSpace(ExitAddress))
            {
                try
                {
                    ExitAddresses.Remove(ExitAddress);

                }
                catch (Exception)
                {
                }
            }
        }

        //const int maxRetriesCount = 3;
        //private int currentRetryNumber = 0;
        protected override async Task<bool> Initialize(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            _timeoutChecker = new Stopwatch();
            _timeoutChecker.Start();
            while (!Connected)
            {
                await Task.Delay(100, ct);
                CheckIfConnected();
                if (!Connected && _timeoutChecker.ElapsedMilliseconds > Timeout)
                {
                    Logger.Warn($"Proxy launch on port {SocksPort} has timed out");
                    State = ProcessState.InitializationFailed;
                    break;
                }
            }
            UpdateProxy();
            if (await UpdateExitAddress(ct))
            {
                return true;
            }
            return false;
        }

        private void CheckIfConnected()
        {
            Connected = _consoleOutput.ToString().Contains("Bootstrapped 100%: Done");
        }

        private void UpdateProxy()
        {
            var proxyConfig = new ProxyConfig(IPAddress.Loopback, HttpPort, IPAddress.Loopback, SocksPort, ProxyConfig.SocksVersion.Five);
            Proxy = new SocksWebProxy(proxyConfig);
        }

        private async Task<bool> UpdateExitAddress(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            ExitAddress = await HttpHelpers.WhatIsMyIp(Proxy);
            Logger.Info($"Proxy is connected with IP address {ExitAddress}");
            if (string.IsNullOrWhiteSpace(ExitAddress))
            {
                Logger.Warn($"Proxy on port {SocksPort} has empty exit address, retrying.");
                return false;
            }
            if (BlockedIps.Contains(ExitAddress))
            {
                Logger.Warn($"Proxy on port {SocksPort} has an blocked exit address, retrying.");
                return false;
            }
            if (ExitAddresses.Contains(ExitAddress))
            {
                Logger.Warn($"Proxy on port {SocksPort} has duplicate exit address, retrying.");
                return false;
            }
            Country = GeoIpHelper.GetCountry(ExitAddress);
            //if (TorCountries != null && TorCountries.Count > 0 && (Country.IsoCode == null || !TorCountries.Contains(Country.IsoCode)))
            //{
            //    var countries = "{" + string.Join(",", TorCountries) + "}";
            //    Logger.Warn($"Proxy on port {SocksPort} has was supposed to be in {countries}, but the resulting connection ended up in {{{Country.IsoCode ?? "Unknown Country"}}}, retrying.");
            //    return false;
            //}
            ExitAddresses.Add(ExitAddress);
            Logger.Info($"Proxy on port {SocksPort} is now connected to {Country.Name} [{Country.IsoCode}] with ip address {ExitAddress}");
            return true;
        }

        protected override void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            base.Process_OutputDataReceived(sender, e);
            if (e.Data != null)
            {
                _consoleOutput.Append(e.Data);
            }
        }
    }

}
