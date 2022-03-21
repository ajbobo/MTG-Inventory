using System;
using System.Collections.Generic;
using Google.Cloud.Firestore;

namespace MTG_CLI 
{
        
    // [FirestoreData(ConverterType = typeof(CTC_Converter))]
    // public class CardTypeCount
    // {
    //     public int Count { get; set; }
    //     public bool? Foil { get; set; }
    //     public bool? PreRelease { get; set; }
    //     public bool? Spanish { get; set; }

    //     public CardTypeCount()
    //     {
    //         this.Count = 0;
    //     }

    //     public CardTypeCount(bool? foil, bool? preRelease, bool? spanish)
    //     {
    //         this.Foil = foil;
    //         this.PreRelease = preRelease;
    //         this.Spanish = spanish;
    //     }
    // }

    // public class CTC_Converter : IFirestoreConverter<CardTypeCount>
    // {
    //     public object ToFirestore(CardTypeCount ctc)
    //     {
    //         Dictionary<string, object> entry = new Dictionary<string, object>
    //         {
    //             { "Count", ctc.Count },
    //         };

    //         if (ctc.Foil.HasValue) entry.Add("Foil", ctc.Foil);
    //         if (ctc.PreRelease.HasValue) entry.Add("PreRelease", ctc.PreRelease);
    //         if (ctc.Spanish.HasValue) entry.Add("Spanish", ctc.Spanish);

    //         return entry;
    //     }

    //     public CardTypeCount FromFirestore(object value)
    //     {
    //         return null;
    //     }
    // }

    [FirestoreData]
    public class MTG_Card
    {
        // [FirestoreProperty] public List<CardTypeCount>? Counts { get; private set; }
        [FirestoreProperty] public string Name { get; set; } = "";
        [FirestoreProperty] public string SetCode { get; set; } = "unk";
        [FirestoreProperty] public string Set { get; set; } = "unk";
        [FirestoreProperty] public string CollectorNumber { get; set; } = "0";

        public MTG_Card()
        {
            // Counts = new List<CardTypeCount>();
        }
    }
}