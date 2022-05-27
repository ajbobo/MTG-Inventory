using System;
using System.Collections.Generic;
using System.Text;
using Google.Cloud.Firestore;

namespace TestDB
{
    [FirestoreData]
    public class UserInventory
    {
        [FirestoreProperty]
        public List<MTG_Set> Sets { get; set; } = new();

        public override string ToString()
        {
            StringBuilder builder = new("--- User Inventory ---");
            foreach (MTG_Set set in Sets)
            {
                builder.Append("\r\n" + set);
            }
            return builder.ToString();
        }
    }

    [FirestoreData]
    public class MTG_Set
    {
        [FirestoreProperty]
        public string Name { get; set; } = "";

        [FirestoreProperty]
        public string Code { get; set; } = "";

        // [FirestoreProperty]
        public List<MTG_Card> Cards { get; set; } = new();

        public override string ToString()
        {
            StringBuilder builder = new(string.Format("{0} ({1})", Name, Code));
            foreach (MTG_Card card in Cards)
            {
                builder.Append("\r\n" + card);
            }
            return builder.ToString();
        }
    }

    [FirestoreData]
    public class MTG_Card
    {
        /*[FirestoreProperty]*/ public List<CardTypeCount> Counts { get; private set; } = new();
        [FirestoreProperty] public string Name { get; set; } = "";
        [FirestoreProperty] public string CollectorNumber { get; set; } = "0";

        public MTG_Card()
        {
            // Default values are fine
        }

        public MTG_Card(MTG_Card_Legacy original)
        {
            this.Counts = original.Counts;
            this.Name = original.Name;
            this.CollectorNumber = original.CollectorNumber;
        }

        public override string ToString()
        {
            StringBuilder builder = new(string.Format("\t{0} ({1})", Name, CollectorNumber));
            foreach (CardTypeCount ctc in Counts)
            {
                builder.Append("\r\n" + ctc);
            }
            return builder.ToString();
        }
    }

    public class MTG_Card_Legacy : MTG_Card
    {
        public string SetCode { get; set; } = "unk";
        public string Set { get; set; } = "unk";
        public string UUID { get; set; } = "";
    }


    [FirestoreData]
    public class CardTypeCount
    {
        [FirestoreProperty] public int Count { get; set; } = 0;
        [FirestoreProperty] public List<string> Attrs { get; set; } = new();

        public override string ToString()
        {
            string desc = (Attrs.Count == 0 ? "Default" : String.Join("|", Attrs));
            return string.Format("\t\t{0} : {1}", desc, Count);
        }
    }
}