using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using POGOProtos.Enums;

namespace BulbaGO.Base.BotDataManagement
{
    public class PokedexEntry
    {
        [BsonRepresentation(BsonType.String)]
        public PokemonId PokemonId { get; set; }
        public int TimesEncountered { get; set; }
        public int TimesCaptured { get; set; }
        public int EvolutionStones { get; set; }
        public int EvolutionStonePieces { get; set; }
    }
}