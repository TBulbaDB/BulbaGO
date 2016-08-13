using System;
using System.IO;
using System.Net;
using BulbaGO.Base.ProcessManagement;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BulbaGO.Base.Bots
{
    public class NecroBotConfig : IBotConfig
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(NecroBotConfig).Name);

        private static readonly string _botFolder;
        private static readonly string _botConfigFolder;
        private static readonly string _botExecutablePath;

        static NecroBotConfig()
        {
            _botFolder = Path.Combine(Environment.CurrentDirectory, "Bots", "NecroBot");
            _botConfigFolder = Path.Combine(_botFolder, "Config");
            _botExecutablePath = Path.Combine(_botFolder, "NecroBot.exe");
        }

        public void CreateBotConfig(Bot bot)
        {
            var defaultConfigFilePath = Path.Combine(_botConfigFolder, "config.json");
            var defaultAuthFilePath = Path.Combine(_botConfigFolder, "auth.json");

            var defaultConfig = JsonConvert.DeserializeObject(File.ReadAllText(defaultConfigFilePath)) as JObject;
            var defaultAuth = JsonConvert.DeserializeObject(File.ReadAllText(defaultAuthFilePath)) as JObject;

            if (defaultConfig == null)
            {
                Logger.Error("Could not load default NecroBot config.json");
                throw new Exception("Could not load default NecroBot config.json");
            }

            if (defaultAuth == null)
            {
                Logger.Error("Could not load default NecroBot auth.json");
                throw new Exception("Could not load default NecroBot auth.json");
            }

            foreach (var p in defaultConfig.Properties())
            {
                switch (p.Name)
                {
                    case "DefaultLatitude":
                        p.Value = bot.Location.Latitude;
                        break;

                    case "DefaultLongitude":
                        p.Value = bot.Location.Longitude;
                        break;

                    case "MaxTravelDistanceInMeters":
                        p.Value = bot.Location.MaxWalkDistance;
                        break;
                }
            }

            foreach (var p in defaultAuth.Properties())
            {
                switch (p.Name)
                {
                    case "AuthType":
                        p.Value = bot.AuthType.ToString().ToLowerInvariant();
                        break;

                    case "GoogleUsername":
                        p.Value = bot.AuthType == AuthType.Google ? bot.Username : null;
                        break;

                    case "GooglePassword":
                        p.Value = bot.AuthType == AuthType.Google ? bot.Password : null;
                        break;

                    case "PtcUsername":
                        p.Value = bot.AuthType == AuthType.Ptc ? bot.Username : null;
                        break;

                    case "PtcPassword":
                        p.Value = bot.AuthType == AuthType.Ptc ? bot.Password : null;
                        break;

                    case "UseProxy":
                        p.Value = true;
                        break;

                    case "UseProxyHost":
                        p.Value = IPAddress.Loopback.ToString();
                        break;

                    case "UseProxyPort":
                        p.Value = bot.ProxyProcess.HttpPort;
                        break;
                }
            }

            var accountConfigPath = Path.Combine(_botFolder, bot.Username, "Config");
            Directory.CreateDirectory(accountConfigPath);
            File.WriteAllText(Path.Combine(accountConfigPath, "config.json"), defaultConfig.ToString());
            File.WriteAllText(Path.Combine(accountConfigPath, "auth.json"), defaultAuth.ToString());
        }

        public void ProcessOutputData(BotProcess botProcess, string data)
        {
            if (data.Contains("(ATTENTION) No usable PokeStops found in your area. Is your maximum distance too small?"))
            {
                botProcess.Logger.Error("Bot reported no pokestops around, possible ip ban, restarting bot.");
                botProcess.State = ProcessState.Error;
                //TerminateProcess();
                //_bot.Restart();
                return;
            }
            if (data.Contains("INVALID PROXY"))
            {
                botProcess.Logger.Error("Bot reported invalid proxy, restarting bot.");
                //TerminateProcess();
                //_bot.Restart();
                botProcess.State = ProcessState.Error;
                return;
            }
            if (data.Contains("ERROR"))
            {
                botProcess.Logger.Error("Bot reported an error, restarting bot.");
                //TerminateProcess();
                //_bot.Restart();
                botProcess.State = ProcessState.Error;
                return;
            }
        }

        public string BotName { get; } = "NecroBot";
        public string BotFolder { get; } = _botFolder;
        public string BotConfigFolder { get; } = _botConfigFolder;
        public string BotExecutablePath { get; } = _botExecutablePath;
    }
}
