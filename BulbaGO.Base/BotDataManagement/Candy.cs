using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using POGOProtos.Enums;

namespace BulbaGO.Base.BotDataManagement
{
    public class Candy
    {
        [BsonRepresentation(BsonType.String)]
        public PokemonFamilyId PokemonFamilyId { get; set; }
        public int Count { get; set; }
    }
}