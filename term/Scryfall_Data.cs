using Newtonsoft.Json;

namespace Scryfall
{
    public class Set
    {
        public string Code { get; set; } = "";
        public string Name { get; set; } = "<unknown>";

        override public string ToString()
        {
            return Name;
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
        [JsonProperty("rarity")] public string Rarity { get; set; } = "";
        [JsonProperty("collector_number")] public string CollectorNumber { get; set; } = "0";
        [JsonProperty("set_name")] public string SetName {get; set;} = "";
        [JsonProperty("set")] public string SetCode { get; set; } = "";
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