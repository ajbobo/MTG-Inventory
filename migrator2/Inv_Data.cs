using System;
using Google.Cloud.Firestore;
using Newtonsoft.Json;

namespace Migrator
{
    [FirestoreData]
    public class Inv_Card
    {
        [FirestoreProperty] public string Name { get; set; } = "<unknown>";
        [FirestoreProperty] public string CollectorNumber { get; set; } = "0";
        [FirestoreProperty] public List<Inv_CardTypeCount> Counts { get; set; } = new();

        public Inv_Card() { }

        public Inv_Card(Json_Card orig)
        {
            this.Name = orig.Name;
            this.CollectorNumber = orig.CollectorNumber;
            foreach (Json_CardTypeCount ctc in orig.Counts)
            {
                Counts.Add(new() { Attrs = ctc.Attrs, Count = ctc.Count });
            }
        }
    }

    [FirestoreData]
    public class Inv_CardTypeCount
    {
        [FirestoreProperty] public int Count { get; set; } = 0;
        [FirestoreProperty] public List<string> Attrs { get; set; } = new();
    }
}