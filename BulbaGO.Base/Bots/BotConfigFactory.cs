using System;

namespace BulbaGO.Base.Bots
{
    public static class BotConfigFactory
    {
        public static IBotConfig GetInstance(Bot bot)
        {
            IBotConfig botConfig;
            switch (bot.BotType)
            {
                case BotType.NecroBot:
                    botConfig= new NecroBotConfig();
                    break;
                case BotType.PokeMobBot:
                    botConfig= new PokeMobBotConfig();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(bot.BotType), bot.BotType, null);
            }
            botConfig.CreateBotConfig(bot);
            return botConfig;
        }
    }
}