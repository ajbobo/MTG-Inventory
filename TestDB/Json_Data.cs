using System;
using System.Collections.Generic;
using System.Text;
using Google.Cloud.Firestore;

namespace TestDB
{

    [FirestoreData]
    public class CardTypeCount : IComparable<CardTypeCount>
    {
        [FirestoreProperty] public int Count { get; set; } = 0;
        [FirestoreProperty] public List<string> Attrs { get; set; } = new();

        public CardTypeCount()
        {
            this.Count = 0;
        }

        public string GetAttrs()
        {
            StringBuilder builder = new();
            foreach (string attr in Attrs)
            {
                if (builder.Length > 0)
                    builder.Append(" | ");
                builder.AppendFormat("{0}", attr);
            }
            if (builder.Length == 0)
                builder.Append("Standard");
            return builder.ToString();
        }

        public override string ToString()
        {
            return string.Format("{0} - {1}", Count, GetAttrs());
        }

        public int CompareTo(CardTypeCount? other)
        {
            int cnt1 = this.Attrs.Count;
            int cnt2 = other?.Attrs.Count ?? 0;

            if (cnt1 < cnt2)
                return -1;
            else if (cnt1 == cnt2)
                return 0;
            else if (cnt1 > cnt2)
                return 1;

            return 0;
        }
    }

    [FirestoreData]
    public class MTG_Card
    {
        [FirestoreProperty] public List<CardTypeCount> Counts { get; private set; }
        [FirestoreProperty] public string Name { get; set; } = "";
        [FirestoreProperty] public string SetCode { get; set; } = "unk";
        [FirestoreProperty] public string Set { get; set; } = "unk";
        [FirestoreProperty] public string CollectorNumber { get; set; } = "0";
        public string UUID { get; set; } = "";

        public MTG_Card()
        {
            Counts = new();
        }

    }
}