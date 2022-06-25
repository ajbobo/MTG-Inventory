using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Scryfall
{
    public class SetListResponse
    {
        // There's more in the response, but this is all I care about
        public List<Set> Data { get; set; } = new();
    }

    public class Set
    {
        public string Code { get; set; } = "";
        public string Name { get; set; } = "<unknown>";

        override public string ToString()
        {
            return Name;
        }

        public static Set NONE = new Set();

        public enum SetType
        {
            NONE,
            CORE,
            EXPANSION,
            MASTERS,
            ALCHEMY,
            MASTERPIECE,
            ARSENAL,
            FROM_THE_VAULT,
            SPELLBOOK,
            PREMIUM_DECK,
            DUEL_DECK,
            DRAFT_INNOVATION,
            TREASURE_CHEST,
            COMMANDER,
            PLANECHASE,
            ARCHENEMY,
            VANGUARD,
            FUNNY,
            STARTER,
            BOX,
            PROMO,
            TOKEN,
            MEMORABILIA
        }
    }

    public class CardListResponse
    {
        public bool Has_More { get; set; } = false;
        public List<Card> Data { get; set; } = new();
    }

    public class Card
    {
        // There are tons of other fields returned, these are just the ones I need
        //     See https://scryfall.com/docs/api/cards
        [JsonProperty("color_identity")] public List<string> ColorIdentity { get; set; } = new();
        [JsonProperty("mana_cost")] public string ManaCost { get; set; } = "";
        [JsonProperty("name")] public string Name { get; set; } = "<unknown>";
        [JsonProperty("rarity")][JsonConverter(typeof(StringEnumConverter))] public TestDB.CardRarity Rarity { get; set; } = TestDB.CardRarity.Common;
        [JsonProperty("collector_number")] public string CollectorNumber { get; set; } = "0";
        [JsonProperty("type_line")] public string TypeLine { get; set; } = "";
        [JsonProperty("oracle_text")] public string Text { get; set; } = "";
        [JsonProperty("card_faces")] public List<CardFace> Faces { get; set; } = new();

        public override string ToString()
        {
            return Name;
        }
    }

    public class CardFace
    {
        [JsonProperty("name")] public string Name { get; set; } = "";
        [JsonProperty("oracle_text")] public string Text { get; set; } = "";
    }
}