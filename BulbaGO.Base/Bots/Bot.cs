using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BulbaGO.Base.Devices;
using BulbaGO.Base.GeoLocation;
using BulbaGO.Base.MongoDB;
using BulbaGO.Base.ProcessManagement;
using log4net;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace BulbaGO.Base.Bots
{
    public class BotProgress
    {
        public string BotTitle { get; set; }
    }

    public enum BotType
    {
        NecroBot,
        PokeMobBot
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
        public DeviceSettings DeviceSettings { get; set; }

        public BotType BotType { get; set; }

        public IBotConfig BotConfig { get; set; }


        [BsonIgnore]
        public SocksWebProxyProcess ProxyProcess { get; set; }

        [BsonIgnore]
        public BotProcess BotProcess { get; set; }

        [BsonIgnore]
        public string BotProcessTitle => BotProcess?.WindowTitle;

        [BsonIgnore]
        public ILog Logger { get; private set; }

        [BsonIgnore]
        public IProgress<BotProgress> Progress { get; private set; }

        [BsonIgnore]
        public BotState State { get; private set; }

        private CancellationTokenSource _cts;
        private CancellationToken _ct;

        public static async Task<Bot> GetInstance(AuthType authType, string username, string password,
            string twoLetterIsoCountryCode)
        {
            var cts = new CancellationTokenSource();
            var ct = cts.Token;
            ct.ThrowIfCancellationRequested();
            try
            {
                var bot = await GetBotFromMongoDb(username, ct);

                if (bot != null && (bot.Location == null || bot.TwoLetterIsoCountryCode != twoLetterIsoCountryCode || bot.Location.TwoLetterIsoCountryCode!=twoLetterIsoCountryCode))
                {
                    bot.TwoLetterIsoCountryCode = twoLetterIsoCountryCode;
                    bot.Location = await StartLocationProvider.GetRandomStartLocation(twoLetterIsoCountryCode, ct);
                    bot.Save();
                }
                if (bot != null && bot.DeviceSettings == null)
                {
                    bot.DeviceSettings = new DeviceSettings();
                    bot.Save();
                }
                if (bot == null)
                {
                    bot = new Bot();
                    bot.AuthType = authType;
                    bot.Username = username;
                    bot.Password = password;
                    bot.TwoLetterIsoCountryCode = twoLetterIsoCountryCode;
                    bot.Location = await StartLocationProvider.GetRandomStartLocation(twoLetterIsoCountryCode, ct);
                    bot.DeviceSettings = new DeviceSettings();
                    bot.Save();
                }
                bot._cts = cts;
                bot._ct = ct;
                bot.Logger = LogManager.GetLogger(bot.Username);
                return bot;
            }
            catch (OperationCanceledException osc)
            {

            }
            return null;

        }

        private static readonly FilterDefinitionBuilder<Bot> FilterBuilder =
            Builders<Bot>.Filter;

        //private static readonly UpdateDefinitionBuilder<Bot> UpdateBuilder = Builders<Bot>.Update;

        private static async Task<Bot> GetBotFromMongoDb(string username, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var filter = FilterBuilder.Eq(b => b.Username, username);
            return await MongoHelper.GetCollection<Bot>().Find(filter).FirstOrDefaultAsync(ct);
        }

        public void Save()
        {
            MongoHelper.GetCollection<Bot>().ReplaceOne(FilterBuilder.Eq(b => b.Username, Username), this, new UpdateOptions { IsUpsert = true });
        }

        private async Task<bool> SetProcesses(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            ProxyProcess = SocksWebProxyProcess.GetInstance(this, TwoLetterIsoCountryCode);
            ProxyProcess.ProcessExited += ProxyProcess_ProcessExited;
            if (await ProxyProcess.Start(ct))
            {
                BotProcess = new BotProcess(this);
                BotProcess.ProcessExited += BotProcessContainer_ProcessExited;
                BotProcess.BotProgressChanged += BotProcess_BotProgressChanged;
                return true;
            }
            return false;
        }

        private void BotProcess_BotProgressChanged(BotProgress progress)
        {
            Progress?.Report(progress);
        }

        private async void ProxyProcess_ProcessExited(AsyncProcess process)
        {
            if (State == BotState.Terminating) return;
            await StopProcesses();
            Logger.Warn("Restarting everything in 10 seconds.");

            State = BotState.BotError;
            await Task.Delay(10000);
            await Start(BotType);
        }

        private async void BotProcessContainer_ProcessExited(AsyncProcess process)
        {
            if (State == BotState.Terminating) return;
            await StopProcesses();
            State = BotState.BotError;
            Logger.Warn("Restarting everything in 10 seconds.");
            await Task.Delay(10000);
            await Start(BotType);
        }

        private async Task<bool> StopProcesses()
        {
            State = BotState.Terminating;
            if (BotProcess != null)
            {
                await BotProcess.Stop();
                BotProcess = null;
            }
            if (ProxyProcess != null)
            {
                await ProxyProcess.Stop();
                ProxyProcess = null;
            }
            await Task.Delay(500);

            
            return true;
        }


        public async Task Start(BotType botType, IProgress<BotProgress> progress = null, IntPtr botProcessParent = default(IntPtr))
        {
            if (State == BotState.Running) return;

            State = BotState.Running;
            Progress = progress;
            _ct.ThrowIfCancellationRequested();
            BotType = botType;
            try
            {
                if (await SetProcesses(_ct))
                {
                    BotConfig = BotConfigFactory.GetInstance(this);
                    await BotProcess.Start(_ct, botProcessParent);
                }
            }
            catch (OperationCanceledException oce)
            {

            }
            catch (Exception ex)
            {
                Logger.Error($"An unexpected error occured while launching the bot. [{ex.GetType().Name}] {ex.Message}\r\n{ex.StackTrace}");
                await StopProcesses();
            }
        }

        public async Task Stop()
        {
            _cts.Cancel();
            await StopProcesses();
            _cts = new CancellationTokenSource();
            _ct = _cts.Token;

        }

        public async void Restart()
        {
            await StopProcesses();
            await Start(BotType);
        }
    }
}
