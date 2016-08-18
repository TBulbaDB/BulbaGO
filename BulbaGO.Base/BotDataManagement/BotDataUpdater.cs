using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using BulbaGO.Base.Bots;
using BulbaGO.Base.MongoDB;
using log4net;
using MongoDB.Driver;
using POGOLib.Net.Authentication;
using POGOLib.Pokemon.Data;

namespace BulbaGO.Base.BotDataManagement
{
    public static class BotDataUpdater
    {
        public static ILog Logger = LogManager.GetLogger("BotDataUpdater");
        public static Task<bool> UpdateInventory(string username)
        {
            var timeout = new Stopwatch();
            timeout.Start();
            Logger.Info($"Attempting to update inventory for {username}");
            var tcs = new TaskCompletionSource<bool>();
            var bot = MongoHelper.GetCollection<Bot>().Find(b => b.Username == username).FirstOrDefault();
            if (bot == null)
            {
                Logger.Warn($"Couldn't find the account information for {username}");
                tcs.SetResult(false);
                return tcs.Task;
            };
            var session = Login.GetSession(bot.Username, bot.Password, LoginProvider.PokemonTrainerClub,
                bot.Location.Latitude, bot.Location.Longitude);


            session.Player.Inventory.Update += (sender, eventArgs) =>
            {
                Logger.Info($"Received inventory for {username}");
                var player = session.Player;
                var botData = new BotData();
                botData.Username = username;
                botData.Stats = player.Stats;
                botData.UpdateInventory(player.Inventory);
                MongoHelper.GetCollection<BotData>().ReplaceOne(b => b.Username == username, botData, new UpdateOptions { IsUpsert = true });
                // Access updated inventory: session.Player.Inventory
                session.Shutdown();
                session.Dispose();
                Logger.Info($"Updated inventory for {username}");
                tcs.SetResult(true);
            };
            session.Map.Update += (sender, eventArgs) =>
            {
                // Access updated map: session.Map
                Console.WriteLine("Map was updated.");
            };


            session.Startup();
            //           session.Shutdown();
            return tcs.Task;
        }



    }
}
