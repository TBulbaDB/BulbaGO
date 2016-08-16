using System.Linq;
using MongoDB.Bson.Serialization.Attributes;
using POGOProtos.Data.Player;
using POGOProtos.Enums;

namespace BulbaGO.Base.BotDataManagement
{
    [BsonIgnoreExtraElements]
    public class BotData
    {
        [BsonId]
        public string Username { get; set; }
        public PlayerStats Stats { get; set; }

        public BotInventory Inventory { get; set; }


        public void UpdateInventory(POGOLib.Pokemon.Inventory inventory)
        {
            Inventory = new BotInventory();
            var inventoryItems = inventory.InventoryItems.Select(i => i.InventoryItemData).ToList();
            foreach (var inventoryItem in inventoryItems)
            {
                if (inventoryItem.Candy != null)
                {
                    Inventory.Candies.Add(new Candy { PokemonFamilyId = inventoryItem.Candy.FamilyId, Count = inventoryItem.Candy.Candy_ });
                }
                if (inventoryItem.PokedexEntry != null)
                {
                    var entry = inventoryItem.PokedexEntry;
                    Inventory.PokedexEntries.Add(new PokedexEntry
                    {
                        PokemonId = entry.PokemonId,
                        TimesEncountered = entry.TimesEncountered,
                        TimesCaptured = entry.TimesCaptured,
                        EvolutionStones = entry.EvolutionStones,
                        EvolutionStonePieces = entry.EvolutionStonePieces
                    });
                }

                if (inventoryItem.PokemonData != null)
                {
                    var data = inventoryItem.PokemonData;
                    if (data.IsEgg) continue;
                    if (data.PokemonId == PokemonId.Missingno) continue;
                    var pokemon = new Pokemon
                    {
                        PokemonId = data.PokemonId,
                        Cp = data.Cp,
                        Stamina = data.Stamina,
                        StaminaMax = data.StaminaMax,
                        Move1 = data.Move1,
                        Move2 = data.Move2,
                        IsEgg = data.IsEgg,
                        Origin = data.Origin,
                        HeightM = data.HeightM,
                        WeightKg = data.WeightKg,
                        IndividualAttack = data.IndividualAttack,
                        IndividualDefense = data.IndividualDefense,
                        IndividualStamina = data.IndividualStamina,
                        CpMultiplier = data.CpMultiplier,
                        BattlesAttacked = data.BattlesAttacked,
                        BattlesDefended = data.BattlesDefended,
                        NumUpgrades = data.NumUpgrades,
                        AdditionalCpMultiplier = data.AdditionalCpMultiplier,
                        Favorite = data.Favorite,
                        Nickname = data.Nickname,
                        Quality = PokemonInfo.CalculatePokemonPerfection(data)
                    };
                    Inventory.Pokemons.Add(pokemon);

                }
            }
        }
    }
}
