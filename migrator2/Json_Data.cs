using System;
using System.Collections.Generic;
using System.Text;
using Google.Cloud.Firestore;

namespace Migrator
{
    public class Json_CardTypeCount
    {
        public int Count { get; set; } = 0;
        public List<string> Attrs { get; set; } = new();

        public Json_CardTypeCount()
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
    }

    public class Json_Card
    {
        public List<Json_CardTypeCount> Counts { get; private set; } = new();
        public string Name { get; set; } = "";
        public string SetCode { get; set; } = "unk";
        public string CollectorNumber { get; set; } = "0";
    }
}