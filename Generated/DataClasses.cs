using Newtonsoft.Json;

namespace YourNamespace
{
    public class Card
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("casting_cost")]
        public string CastingCost { get; set; }

        [JsonProperty("rarity")]
        public string Rarity { get; set; }

        [JsonProperty("type_line")]
        public string TypeLine { get; set; }

        [JsonProperty("front_text")]
        public string FrontText { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("price_foil")]
        public decimal PriceFoil { get; set; }

        [JsonProperty("set_code")]
        public string SetCode { get; set; }
    }

    public class Set
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("set_name")]
        public string SetName { get; set; }

        [JsonProperty("set_code")]
        public string SetCode { get; set; }
    }

    public class Collection
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("set_code")]
        public string SetCode { get; set; }

        [JsonProperty("collector_number")]
        public string CollectorNumber { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("attrs")]
        public JObject Attrs { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }
    }
}
