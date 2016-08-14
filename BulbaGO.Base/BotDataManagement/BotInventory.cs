using System.Collections.Generic;

namespace BulbaGO.Base.BotDataManagement
{
    public class BotInventory
    {
        public List<Candy> Candies { get; set; } = new List<Candy>();
        public List<PokedexEntry> PokedexEntries { get; set; } = new List<PokedexEntry>();
        public List<Pokemon> Pokemons { get; set; } = new List<Pokemon>();
    }
}