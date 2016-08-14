using System;
using System.IO;
using System.Linq;
using System.Net;
using BulbaGO.Base.ProcessManagement;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BulbaGO.Base.Bots
{
    public class PokeMobBotConfig : IBotConfig
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(NecroBotConfig).Name);

        private static readonly string _botFolder;
        private static readonly string _botConfigFolder;
        private static readonly string _botExecutablePath;

        static PokeMobBotConfig()
        {
            _botFolder = Path.Combine(Environment.CurrentDirectory, "Bots", "PokeMobBot");
            _botConfigFolder = Path.Combine(_botFolder, "Config");
            _botExecutablePath = Path.Combine(_botFolder, "PokeMobBot.exe");
        }

        public void CreateBotConfig(Bot bot)
        {
            var defaultConfigFilePath = Path.Combine(_botConfigFolder, "config.json");
            var defaultAuthFilePath = Path.Combine(_botConfigFolder, "auth.json");

            var defaultConfig = JsonConvert.DeserializeObject(File.ReadAllText(defaultConfigFilePath)) as JObject;
            var defaultAuth = JsonConvert.DeserializeObject(File.ReadAllText(defaultAuthFilePath)) as JObject;

            if (defaultConfig == null)
            {
                Logger.Error("Could not load default PokeMobBot config.json");
                throw new Exception("Could not load default PokeMobBot config.json");
            }

            if (defaultAuth == null)
            {
                Logger.Error("Could not load default PokeMobBot auth.json");
                throw new Exception("Could not load default PokeMobBot auth.json");
            }

            JToken deviceSettings;
            if (defaultConfig.TryGetValue("DeviceSettings", out deviceSettings))
            {
                var props = deviceSettings.Children().Cast<JProperty>().ToList();

                foreach (var propertyInfo in bot.DeviceSettings.GetType().GetFields())
                {
                    var prop = props.FirstOrDefault(p => p.Name == propertyInfo.Name);
                    if (prop != null)
                    {
                        prop.Value = JToken.FromObject(propertyInfo.GetValue(bot.DeviceSettings));
                    }
                }
            }

            JToken locationSettings;
            if (defaultConfig.TryGetValue("LocationSettings", out locationSettings))
            {
                var props = locationSettings.Children().Cast<JProperty>().ToList();

                foreach (var prop in props)
                {
                    switch (prop.Name)
                    {

                        case "DefaultLatitude":
                            prop.Value = bot.Location.Latitude;
                            break;
                        case "DefaultLongitude":
                            prop.Value = bot.Location.Longitude;
                            break;
                        case "DefaultAltitude":
                            prop.Value = bot.Location.Altitude;
                            break;
                        case "MaxTravelDistanceInMeters":
                            prop.Value = bot.Location.MaxWalkDistance;
                            break;
                    }
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

                    case "ProxyUri":
                        p.Value = $"http://127.0.0.1:{bot.ProxyProcess.HttpPort}";
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
                botProcess.State = ProcessState.IPBan;
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
                if (data.Contains("Niantic Servers unstable"))
                {
                    return;
                }
                botProcess.Logger.Error("Bot reported an error, restarting bot.");
                //TerminateProcess();
                //_bot.Restart();
                botProcess.State = ProcessState.Error;
                return;
            }
        }

        public string BotName { get; } = "PokeMobBot";
        public string BotFolder { get; } = _botFolder;
        public string BotConfigFolder { get; } = _botConfigFolder;
        public string BotExecutablePath { get; } = _botExecutablePath;
    }
}
