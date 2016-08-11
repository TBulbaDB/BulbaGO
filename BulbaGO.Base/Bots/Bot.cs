using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BulbaGO.Base.GeoLocation;
using BulbaGO.Base.MongoDB;
using BulbaGO.Base.ProcessManagement;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

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

    public enum BotState
    {
        Unknown,
        ProxyProcessStarting,
        ProxyProcessStarted,
        BotProcessStarting,
        BotProcessStarted,
        Running,
        ProxyError,
        BotError,
        Terminating,
        Terminated
    }

    public class Bot
    {
        [BsonId]
        public string Username { get; set; }
        [BsonRepresentation(BsonType.String)]
        public AuthType AuthType { get; set; }
        public string Password { get; set; }
        public string TwoLetterIsoCountryCode { get; set; }
        public StartLocation Location { get; set; }

        [BsonIgnore]
        public BotType BotType { get; set; }

        [BsonIgnore]
        public SocksWebProxyContainer ProxyContainer { get; set; }

        [BsonIgnore]
        public BotProcessContainer BotProcessContainer { get; set; }

        public static async Task<Bot> GetInstance(AuthType authType, string username, string password,
            string twoLetterIsoCountryCode)
        {

            var bot = await GetBotFromMongoDb(username);
            if (bot != null && bot.Location == null)
            {
                bot.Location = StartLocationProvider.GetRandomStartLocation(twoLetterIsoCountryCode);
                bot.Save();
            }
            if (bot == null)
            {
                bot = new Bot();
                bot.AuthType = authType;
                bot.Username = username;
                bot.Password = password;
                bot.TwoLetterIsoCountryCode = twoLetterIsoCountryCode;
                bot.Location = StartLocationProvider.GetRandomStartLocation(twoLetterIsoCountryCode);
                bot.Save();
            }

            return bot;
        }

        private static readonly FilterDefinitionBuilder<Bot> FilterBuilder =
            Builders<Bot>.Filter;

        //private static readonly UpdateDefinitionBuilder<Bot> UpdateBuilder = Builders<Bot>.Update;

        private static async Task<Bot> GetBotFromMongoDb(string username)
        {
            var filter = FilterBuilder.Eq(b => b.Username, username);
            return await MongoHelper.GetCollection<Bot>().Find(filter).FirstOrDefaultAsync();
        }

        public void Save()
        {
            MongoHelper.GetCollection<Bot>().ReplaceOne(FilterBuilder.Eq(b => b.Username, Username), this, new UpdateOptions { IsUpsert = true });
        }

        private async Task SetContainers()
        {
            ProxyContainer = SocksWebProxyContainer.GetNeWebProxyContainer(Username, TwoLetterIsoCountryCode);
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
            Start(BotType).Wait();
        }

        private async Task ResetContainers()
        {
            if (BotProcessContainer != null)
            {
                BotProcessContainer.TerminateProcess();
                BotProcessContainer = null;
            }
            if (ProxyContainer != null)
            {
                ProxyContainer.TerminateProcess();
                ProxyContainer = null;
            }
            await SetContainers();
        }

        private void ProxyContainer_ProcessExited(object sender, EventArgs e)
        {
            ResetContainers().Wait();
            Start(BotType).Wait();
        }

        public async Task Start(BotType botType)
        {
            BotType = botType;
            await SetContainers();

            if (ProxyContainer != null && ProxyContainer.Connected && BotProcessContainer != null)
            {
                await BotProcessContainer.Start();
            }
        }

        public async void Restart()
        {
            await ResetContainers();
            await Start(BotType);
        }

    }
}
