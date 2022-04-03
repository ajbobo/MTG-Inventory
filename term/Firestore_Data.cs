using System;
using System.Collections.Generic;
using System.Text;
using Google.Cloud.Firestore;

namespace MTG_CLI
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

        public void AdjustCount(int val)
        {
            Count += val;
            if (Count < 0)
                Count = 0;
        }

        public string GetAttrs()
        {
            StringBuilder builder = new();
            foreach (string attr in Attrs)
            {
                if (builder.Length > 0)
                    builder.Append(" | ");
                builder.AppendFormat("{0}", Capitalize(attr));
            }
            if (builder.Length == 0)
                builder.Append("Standard");
            return builder.ToString();
        }

        private string Capitalize(string str)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(char.ToUpper(str[0]));
            builder.Append(str.Substring(1));
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

        public MTG_Card()
        {
            Counts = new();
        }

        public void SortCTCs()
        {
            Counts.Sort();
        }

        public int GetTotalCount()
        {
            int total = 0;
            foreach (CardTypeCount ctc in Counts)
            {
                total += ctc.Count;
            }
            return total;
        }

        public bool HasAttr(params string[] attrs)
        {
            foreach (CardTypeCount ctc in Counts)
            {
                foreach (string attr in attrs)
                {
                    if (ctc.Attrs.Contains(attr))
                        return true;
                }
            }
            return false;
        }

        public bool HasOtherAttr(params string[] attrs)
        {
            List<string> attrList = new List<string>(attrs);
            foreach (CardTypeCount ctc in Counts)
            {
                foreach (string attr in ctc.Attrs)
                {
                    if (!attrList.Contains(attr))
                        return true;
                }
            }
            return false;
        }
    }
}