using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BulbaGO.Base.SocksProxy;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BulbaGO.Base.Bots
{
    public class NecroBot : IBotConfigCreator
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(NecroBot));

        public static readonly string BotFolder;
        private static readonly string BotConfigFolder;
        public static readonly string BotExecutablePath;

        static NecroBot()
        {
            BotFolder = Path.Combine(Environment.CurrentDirectory, "Bots", "NecroBot");
            BotConfigFolder = Path.Combine(BotFolder, "Config");
            BotExecutablePath = Path.Combine(BotFolder, "NecroBot.exe");
        }

        public static void CreateBotConfig(Bot bot)
        {
            var defaultConfigFilePath = Path.Combine(BotConfigFolder, "config.json");
            var defaultAuthFilePath = Path.Combine(BotConfigFolder, "auth.json");

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
                        p.Value = bot.AuthType==AuthType.Google?bot.Username:null;
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
                        p.Value = bot.ProxyContainer.HttpPort;
                        break;
                }
            }

            var accountConfigPath = Path.Combine(BotFolder, bot.Username, "Config");
            Directory.CreateDirectory(accountConfigPath);
            File.WriteAllText(Path.Combine(accountConfigPath, "config.json"), defaultConfig.ToString());
            File.WriteAllText(Path.Combine(accountConfigPath, "auth.json"), defaultAuth.ToString());
        }

    }
}
