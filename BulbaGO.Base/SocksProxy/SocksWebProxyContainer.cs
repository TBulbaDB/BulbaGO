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
    public enum ProxyProcessState
    {
        Unknown,
        Starting,
        Started,
        CheckingExitAddress,
        Connected,
        CouldNotConnect,
        Terminating,
        Terminated,
        CouldNotStart
    }

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

        private const int MinPort = 9051;
        private const int MaxPort = 10050;
        private static readonly Random PortRandomizer = new Random();
        private static readonly HashSet<int> UsedPorts = new HashSet<int>();
        private static readonly HashSet<string> ExitAddresses = new HashSet<string>();

        public static SocksWebProxyContainer GetNeWebProxyContainer(string botName)
        {
            return GetNeWebProxyContainer(botName, new List<string>());
        }

        public static SocksWebProxyContainer GetNeWebProxyContainer(string botName, string isoTwoLetterCountryCode)
        {
            return GetNeWebProxyContainer(botName, new List<string> { isoTwoLetterCountryCode });
        }

        public static SocksWebProxyContainer GetNeWebProxyContainer(string botName, List<string> isoTwoLetterCountryCodes)
        {
            var proxyPort = PortRandomizer.Next(MinPort, MaxPort);
            while (UsedPorts.Contains(proxyPort) || IpTools.IsPortInUse(proxyPort) || IpTools.IsPortInUse(proxyPort + 1000))
            {
                proxyPort = PortRandomizer.Next(MinPort, MaxPort);
            }
            UsedPorts.Add(proxyPort);
            return new SocksWebProxyContainer { TorCountries = isoTwoLetterCountryCodes, SocksPort = proxyPort, BotName = botName};
        }

        private SocksWebProxyContainer()
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
        public string BotName { get; private set; }

        public ProxyProcessState State { get; private set; }
        public event EventHandler ProcessExited;

        private bool _exitRequested;
        private StringBuilder _consoleOutput;
        private Stopwatch _timeoutChecker;



        public async Task Start(int retries = 3, long timeout = 30000)
        {
            State = ProxyProcessState.Starting;
            for (int i = 0; i <= retries; i++)
            {
                Logger.Info($"{BotName} Launching Tor on Port {SocksPort}");
                LaunchTorInstance();
                _timeoutChecker = new Stopwatch();
                _timeoutChecker.Start();
                while (!Connected)
                {
                    await Task.Delay(100);
                    CheckIfConnected();
                    if (!Connected && _timeoutChecker.ElapsedMilliseconds > timeout)
                    {
                        Logger.Warn($"{BotName} Proxy launch on port {SocksPort} has timed out, retrying.");
                        break;
                    }
                }
                if (!Connected) continue;
                UpdateProxy();
                if (await UpdateExitAddress())
                {
                    State = ProxyProcessState.Connected;
                    return;
                }
            }
            State = ProxyProcessState.CouldNotConnect;
            Logger.Error($"{BotName} Failed to establish a tor connection with the requested parameters");

        }

        private void UpdateProxy()
        {
            var proxyConfig = new ProxyConfig(IPAddress.Loopback, HttpPort, IPAddress.Loopback, SocksPort, ProxyConfig.SocksVersion.Five);
            Proxy = new SocksWebProxy(proxyConfig);
        }

        private async Task<bool> UpdateExitAddress()
        {
            State = ProxyProcessState.CheckingExitAddress;
            ExitAddress = await HttpHelpers.WhatIsMyIp(Proxy);
            if (string.IsNullOrWhiteSpace(ExitAddress))
            {
                Logger.Warn($"{BotName} Proxy on port {SocksPort} has empty exit address, retrying.");
                return false;
            }
            if (ExitAddresses.Contains(ExitAddress))
            {
                Logger.Warn($"{BotName} Proxy on port {SocksPort} has duplicate exit address, retrying.");
                return false;
            }
            Country = GeoIpHelper.GetCountry(ExitAddress);
            if (TorCountries != null && TorCountries.Count > 0 && (Country.IsoCode == null || !TorCountries.Contains(Country.IsoCode)))
            {
                var countries = "{" + string.Join(",", TorCountries) + "}";
                Logger.Warn($"{BotName} Proxy on port {SocksPort} has was supposed to be in {countries}, but the resulting connection ended up in {Country.IsoCode ?? "Unknown Country"}, retrying.");
                return false;
            }
            ExitAddresses.Add(ExitAddress);
            State = ProxyProcessState.Connected;
            Logger.Info($"{BotName} Proxy on port {SocksPort} is now connected to {Country.Name} [{Country.IsoCode}] with ip address {ExitAddress}");
            return true;
        }


        public void TerminateProcess()
        {
            State = ProxyProcessState.Terminating;
            if (TorProcess == null || TorProcess.HasExited)
            {
                State = ProxyProcessState.Unknown;
                return;
            }
            try
            {
                var torProcess = TorProcess;
                _exitRequested = true;
                torProcess.Close();
                torProcess.WaitForExit(100);
                if (!torProcess.HasExited)
                {
                    TorProcess.Kill();
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"{BotName} An error occured while killing the tor process: ({ex.GetType().Name}) {ex.Message}");
            }
            State = ProxyProcessState.Terminated;
        }

        private void LaunchTorInstance()
        {
            State = ProxyProcessState.Starting;
            _consoleOutput = new StringBuilder();
            
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

            TorProcess.ErrorDataReceived += Process_ErrorDataReceived;
            TorProcess.Exited += Process_Exited;
            var started = TorProcess.Start();
            if (!started)
            {
                //you may allow for the process to be re-used (started = false) 
                //but I'm not sure about the guarantees of the Exited event in such a case
                State = ProxyProcessState.CouldNotStart;
                throw new InvalidOperationException("Could not start process: " + TorProcess);
            }
            ChildProcessTracker.AddProcess(TorProcess);
            TorProcess.BeginOutputReadLine();
            TorProcess.BeginErrorReadLine();
            State = ProxyProcessState.Started;
        }

        private void CheckIfConnected()
        {
            Connected = _consoleOutput.ToString().Contains("Bootstrapped 100%: Done");
        }

        private void ResetRunningState()
        {
            State = ProxyProcessState.Unknown;
            Connected = false;
            TorProcess = null;
            ExitAddresses.Remove(ExitAddress);
            UsedPorts.Remove(SocksPort);
            InUse = false;
        }

        private void Process_Exited(object sender, EventArgs e)
        {
            if (State != ProxyProcessState.Terminating && State != ProxyProcessState.Terminated)
            {
                OnProcessExit();
            }
            ResetRunningState();
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            var process = sender as Process;
            if (process != null && process.HasExited)
            {
                Logger.Warn($"{BotName} Process exited, exit was requested: {_exitRequested}, exit code is {process.ExitCode}, current running state is {Connected}");
                _exitRequested = false;
                return;
            }
            if (!string.IsNullOrWhiteSpace(e.Data))
            {
                Logger.Error($"{BotName}  {e.Data}");
            }

        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                _consoleOutput.Append(e.Data);
            }
            Logger.Debug($"{BotName} {e.Data}");
        }

        public void Dispose()
        {
            Logger.Debug($"{BotName} Disposing");
            TerminateProcess();
        }

        protected virtual void OnProcessExit()
        {
            ProcessExited?.Invoke(this, EventArgs.Empty);
        }
    }
}
