using System;
using Google.Cloud.Firestore;
using Newtonsoft.Json;

namespace TestDB
{
    // This will be serialized to FirestoreDB as an integer - Do not change the order
    public enum CardRarity
    {
        Common,
        Uncommon,
        Rare,
        Mythic,
        Special
    }

    [FirestoreData]
    public class Inv_Set
    {
        [FirestoreProperty][JsonProperty("name")] public string Name { get; set; } = "";
        [FirestoreProperty][JsonProperty("code")] public string Code { get; set; } = "";
        [JsonProperty("cards")] public List<Inv_Card> Cards { get; set; } = new();

        public Inv_Set() { }

        public Inv_Set(Scryfall.Set set)
        {
            this.Code = set.Code;
            this.Name = set.Name;
        }
    }

    [FirestoreData]
    public class Inv_Card
    {
        [FirestoreProperty][JsonProperty("color_identity")] public string ColorIdentity { get; set; } = "";
        [FirestoreProperty][JsonProperty("mana_cost")] public string ManaCost { get; set; } = "";
        [FirestoreProperty][JsonProperty("name")] public string Name { get; set; } = "<unknown>";
        [FirestoreProperty][JsonProperty("rarity")] public CardRarity Rarity { get; set; } = CardRarity.Common;
        [FirestoreProperty][JsonProperty("collector_number")] public string CollectorNumber { get; set; } = "0";
        [FirestoreProperty][JsonProperty("type_line")] public string TypeLine { get; set; } = "";
        [FirestoreProperty][JsonProperty("oracle_text")] public string Text { get; set; } = "";
        [FirestoreProperty][JsonProperty("card_faces")] public List<Inv_CardFace> Faces { get; set; } = new();
        [FirestoreProperty(ConverterType = typeof(CountsConverter))][JsonProperty("counts")] public List<Inv_CardTypeCount> Counts { get; set; } = new();

        public Inv_Card() { }

        public Inv_Card(Scryfall.Card card)
        {
            this.CollectorNumber = card.CollectorNumber;
            this.ColorIdentity = String.Join("", card.ColorIdentity.ToArray());
            this.Faces = new();
            foreach (Scryfall.CardFace face in card.Faces)
                this.Faces.Add(new Inv_CardFace(face));
            this.ManaCost = card.ManaCost;
            this.Name = card.Name;
            this.Rarity = card.Rarity;
            this.TypeLine = card.TypeLine;
            this.Counts.Add(new());
            this.Text = card.Text;
        }
    }

    public class CountsConverter : IFirestoreConverter<List<Inv_CardTypeCount>>
    {
        public object ToFirestore(List<Inv_CardTypeCount> value)
        {
            Dictionary<string, int> res = new();

            int total = 0;
            foreach (Inv_CardTypeCount ctc in value)
            {
                total += ctc.Count;
                res.Add(ctc.Attrs, ctc.Count);
            }
            res.Add("Total", total);

            return res;            
        }

        List<Inv_CardTypeCount> IFirestoreConverter<List<Inv_CardTypeCount>>.FromFirestore(object value)
        {
            throw new NotImplementedException();
        }
    }

    [FirestoreData]
    public class Inv_CardFace
    {
        [FirestoreProperty][JsonProperty("name")] public string Name { get; set; } = "";
        [FirestoreProperty][JsonProperty("oracle_text")] public string Text { get; set; } = "";

        public Inv_CardFace() { }

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