using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using POGOProtos.Enums;

namespace BulbaGO.Base.BotDataManagement
{
    public class Pokemon
    {
        [BsonRepresentation(BsonType.String)]
        public PokemonId PokemonId { get; set; }
        public int Cp { get; set; }
        public int Stamina { get; set; }
        public int StaminaMax { get; set; }
        [BsonRepresentation(BsonType.String)]
        public PokemonMove Move1 { get; set; }
        [BsonRepresentation(BsonType.String)]
        public PokemonMove Move2 { get; set; }
        public bool IsEgg { get; set; }
        public int Origin { get; set; }
        public float HeightM { get; set; }
        public float WeightKg { get; set; }
        public int IndividualAttack { get; set; }
        public int IndividualDefense { get; set; }
        public int IndividualStamina { get; set; }
        public float CpMultiplier { get; set; }
        public int BattlesAttacked { get; set; }
        public int BattlesDefended { get; set; }
        public int NumUpgrades { get; set; }
        public float AdditionalCpMultiplier { get; set; }
        public int Favorite { get; set; }
        public string Nickname { get; set; }
    }
}