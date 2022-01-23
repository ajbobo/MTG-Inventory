using System;
using System.Collections.Generic;
using Google.Cloud.Firestore;

namespace MTG_Inventory 
{
        
    [FirestoreData(ConverterType = typeof(CTC_Converter))]
    public class CardTypeCount
    {
        public CardTypeCount()
        {
            this.Count = 0;
        }

        public CardTypeCount(bool? foil, bool? preRelease, bool? spanish)
        {
            this.Foil = foil;
            this.PreRelease = preRelease;
            this.Spanish = spanish;
        }

        public int Count { get; set; }
        public bool? Foil { get; set; }
        public bool? PreRelease { get; set; }
        public bool? Spanish { get; set; }
    }

    public class CTC_Converter : IFirestoreConverter<CardTypeCount>
    {
        public object ToFirestore(CardTypeCount ctc)
        {
            Dictionary<string, object> entry = new Dictionary<string, object>
            {
                { "Count", ctc.Count },
            };

            if (ctc.Foil.HasValue) entry.Add("Foil", ctc.Foil);
            if (ctc.PreRelease.HasValue) entry.Add("PreRelease", ctc.PreRelease);
            if (ctc.Spanish.HasValue) entry.Add("Spanish", ctc.Spanish);

            return entry;
        }

        public CardTypeCount FromFirestore(object value)
        {
            // This app only every writes to Firestore, it doesn't read
            return null;
        }
    }

    [FirestoreData]
    public class MTG_Card
    {
        [FirestoreProperty] public List<CardTypeCount> Counts { get; private set; }
        [FirestoreProperty] public string Name { get; set; }
        [FirestoreProperty] public string SetCode { get; set; }
        [FirestoreProperty] public string Set { get; set; }
        [FirestoreProperty] public int CollectorNumber { get; set; }

        public MTG_Card()
        {
            Counts = new List<CardTypeCount>();
        }

        public void SetCount(int count, bool? foil, bool? preRelease, bool? spanish)
        {
            foreach (CardTypeCount ctc in Counts)
            {
                if (ctc.Foil == foil && ctc.PreRelease == preRelease && ctc.Spanish == spanish)
                {
                    Console.WriteLine("Incrementing existing collection of \"{0}\" => Foil:{1}  PreRelease:{2}  Spanish:{3}", Name, foil, preRelease, spanish);
                    ctc.Count += count;
                    return;
                }
            }

            if ((foil ?? false) || (preRelease ?? false) || (spanish ?? false))
                Console.WriteLine("Adding special version of \"{0}\" => Foil:{1}  PreRelease:{2}  Spanish:{3}", Name, foil, preRelease, spanish);

            CardTypeCount newCtc = new CardTypeCount(foil, preRelease, spanish);
            newCtc.Count = count;
            Counts.Add(newCtc);
        }
    }
}