using System;
using Newtonsoft.Json;

namespace TestDB
{
    public class Inv_Set
    {
        [JsonProperty("name")] public string Name { get; set; } = "";
        [JsonProperty("code")] public string Code { get; set; } = "";
        [JsonProperty("cards")] public List<Inv_Card> Cards { get; set; } = new();

        public Inv_Set(Scryfall.Set set)
        {
            this.Code = set.Code;
            this.Name = set.Name;
        }
    }

    public class Inv_Card
    {
        [JsonProperty("color_identity")] public List<string> ColorIdentity { get; set; } = new();
        [JsonProperty("mana_cost")] public string ManaCost { get; set; } = "";
        [JsonProperty("name")] public string Name { get; set; } = "<unknown>";
        [JsonProperty("rarity")] public string Rarity { get; set; } = "";
        [JsonProperty("collector_number")] public string CollectorNumber { get; set; } = "0";
        [JsonProperty("type_line")] public string TypeLine { get; set; } = "";
        [JsonProperty("oracle_text")] public string Text { get; set; } = "";
        [JsonProperty("card_faces")] public List<Inv_CardFace> Faces { get; set; } = new();
        [JsonProperty("counts")] public List<Inv_CardTypeCount> Counts { get; set; } = new();

        public Inv_Card(Scryfall.Card card)
        {
            this.CollectorNumber = card.CollectorNumber;
            this.ColorIdentity = card.ColorIdentity;
            this.Faces = new();
            foreach (Scryfall.CardFace face in card.Faces)
                this.Faces.Add(new Inv_CardFace(face));
            this.ManaCost = card.ManaCost;
            this.Name = card.Name;
            this.Rarity = card.Rarity;
            this.TypeLine = card.TypeLine;
            this.Counts.Add(new());
        }
    }

    public class Inv_CardFace
    {
        [JsonProperty("name")] public string Name { get; set; } = "";
        [JsonProperty("oracle_text")] public string Text { get; set; } = "";

        public Inv_CardFace(Scryfall.CardFace face)
        {
            this.Name = face.Name;
            this.Text = face.Text;
        }
    } 

    public class Inv_CardTypeCount
    {
        [JsonProperty("count")] public int Count { get; set; } = 0;
        [JsonProperty("attrs")] public string Attrs { get; set; } = "Standard";
    }

}