using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BulbaGO.Base.GeoLocation;
using BulbaGO.Base.SocksProxy;
using com.LandonKey.SocksWebProxy;
using MongoDB.Bson.Serialization.Attributes;

namespace BulbaGO.Base.Bots
{
    public enum BotType
    {
        NecroBot
    }

    public enum AuthType
    {
        Ptc,
        Google
    }
    public class Bot
    {
        public BotType BotType { get; set; }
        public AuthType AuthType { get; set; }
        [BsonId]
        public string Username { get; set; }
        public string Password { get; set; }
        public string TwoLetterIsoCountryCode { get; set; }
        public StartLocation Location { get; set; }

        [BsonIgnore]
        public SocksWebProxyContainer ProxyContainer { get; set; }

        [BsonIgnore]
        public BotProcessContainer BotProcessContainer { get; set; }

        public static async Task<Bot> CreateNewBot(BotType botType, AuthType authType, string username, string password,
            string twoLetterIsoCountryCode)
        {
            var bot = new Bot();
            bot.BotType = botType;
            bot.AuthType = authType;
            bot.Username = username;
            bot.Password = password;
            bot.TwoLetterIsoCountryCode = twoLetterIsoCountryCode;
            bot.Location = StartLocationProvider.GetRandomStartLocation(twoLetterIsoCountryCode);
            await bot.SetContainers();

            return bot;
        }

        private async Task SetContainers()
        {
            ProxyContainer = SocksWebProxyContainer.GetNeWebProxyContainer(TwoLetterIsoCountryCode);
            ProxyContainer.ProcessExited += ProxyContainer_ProcessExited;
            await ProxyContainer.Start();
            if (ProxyContainer.Connected)
            {
                switch (BotType)
                {
                    case BotType.NecroBot:
                        NecroBot.CreateBotConfig(this);
                        break;
                }
                BotProcessContainer = new BotProcessContainer(this);
                BotProcessContainer.ProcessExited += BotProcessContainer_ProcessExited;
            }
        }

        private void BotProcessContainer_ProcessExited(object sender, EventArgs e)
        {
            ResetContainers().Wait();
            Start().Wait();
        }

        private async Task ResetContainers()
        {
            BotProcessContainer = null;
            ProxyContainer = null;
            await SetContainers();
        }

        private void ProxyContainer_ProcessExited(object sender, EventArgs e)
        {
            ResetContainers().Wait();
            Start().Wait();
        }

        public async Task Start()
        {
            if (ProxyContainer != null && ProxyContainer.Connected && BotProcessContainer != null)
            {
                await BotProcessContainer.Start();
            }
        }

        public async void Restart()
        {
            await ResetContainers();
            await Start();
        }

    }
}
