using System;
using System.Collections.Generic;
using Google.Cloud.Firestore;

namespace MTG_Inventory 
{
        
    [FirestoreData]
    public class CardTypeCount
    {
        [FirestoreProperty] public int Count { get; set; } = 0;
        [FirestoreProperty] public List<string> Attrs { get; set; } = new();

        public CardTypeCount()
        {
            // Do nothing - all initialization is already done - This is here for the upload to Firestore
        }

        public CardTypeCount(bool foil, bool preRelease, bool spanish)
        {
            if (foil) Attrs.Add("foil");
            if (preRelease) Attrs.Add("prerelease");
            if (spanish) Attrs.Add("spanish");
        }

        public bool HasAttr(string attr)
        {
            return Attrs.Contains(attr.ToLower());
        }
    }

    [FirestoreData]
    public class MTG_Card
    {
        [FirestoreProperty] public List<CardTypeCount> Counts { get; private set; }
        [FirestoreProperty] public string Name { get; set; }
        [FirestoreProperty] public string SetCode { get; set; }
        [FirestoreProperty] public string Set { get; set; }
        [FirestoreProperty] public string CollectorNumber { get; set; }

        public MTG_Card()
        {
            Counts = new List<CardTypeCount>();
        }

        public void SetCount(int count, bool foil, bool preRelease, bool spanish)
        {
            foreach (CardTypeCount ctc in Counts)
            {
                if (ctc.HasAttr("foil") == foil && 
                    ctc.HasAttr("prerelease") == preRelease && 
                    ctc.HasAttr("spanish") == spanish)
                {
                    Console.WriteLine("Incrementing existing collection of \"{0}\" => Foil:{1}  PreRelease:{2}  Spanish:{3}", Name, foil, preRelease, spanish);
                    ctc.Count += count;
                    return;
                }
            }

            if (foil || preRelease || spanish)
                Console.WriteLine("Adding special version of \"{0}\" => Foil:{1}  PreRelease:{2}  Spanish:{3}", Name, foil, preRelease, spanish);

            CardTypeCount newCtc = new CardTypeCount(foil, preRelease, spanish);
            newCtc.Count = count;
            Counts.Add(newCtc);
        }
    }
}