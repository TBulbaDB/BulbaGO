using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BulbaGO.Base.MongoDB;
using MongoDB.Driver;

namespace BulbaGO.Base.Bots
{
    public static class BotFactory
    {
        public static async Task<List<Bot>> GetAllBots()
        {
            var result = new List<Bot>();
            var bots = await MongoHelper.GetCollection<Bot>().Find(FilterDefinition<Bot>.Empty).Sort(Builders<Bot>.Sort.Ascending(b => b.Username)).ToListAsync();
            foreach (var bot in bots)
            {
                result.Add(await Bot.GetInstance(bot.AuthType, bot.Username, bot.Password, bot.TwoLetterIsoCountryCode));
            }
            return result;
        }
    }
}
