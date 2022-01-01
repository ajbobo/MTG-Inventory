using System;
using System.IO;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using CsvHelper;

namespace MTG_Inventory
{
    public class MTG_Card
    {
        public int Count { get; set; }

        public string Name { get; set; }

        public string Set { get; set; }

        public int Collector_Number { get; set; }

        public bool Foil { get; set; }

    }

    class UploadInventory
    {

        public static void Main(string[] args)
        {
            List<MTG_Card> theList = new List<MTG_Card>();

            using (var reader = new StreamReader("Inventory.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var results = csv.GetRecords<dynamic>();

                foreach (dynamic record in results)
                {
                    IDictionary<String, Object> card_props = (IDictionary<String, Object>)record;

                    int count = 1, number = 0;
                    int.TryParse(card_props["Count"].ToString(), out count);
                    int.TryParse(card_props["Card Number"].ToString(), out number);

                    MTG_Card card = new MTG_Card
                    {
                        Count = count,
                        Name = card_props["Name"].ToString(),
                        Set = card_props["Edition"]?.ToString(),
                        Foil = card_props["Foil"].ToString().ToLower().Equals("foil"),
                        Collector_Number = number
                    };

                    theList.Add(card);

                    // Console.WriteLine("{0}) {1} = {2}", card.Collector_Number, card.Name, card.Count);

                }
            }
            Console.WriteLine("List size: {0}", theList.Count);
            Console.WriteLine("Done?");
        }
    }
}