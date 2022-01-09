using System;
using System.IO;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using Newtonsoft.Json;
using CsvHelper;
using Scryfall;

namespace MTG_Inventory
{
    public class MTG_Card
    {
        public int Count { get; set; }
        public string Name { get; set; }
        public string SetCode { get; set; }
        public string Set { get; set; }
        public int Collector_Number { get; set; }
        public bool Foil { get; set; }
        public string UniqueID { get; private set; }

        public MTG_Card()
        {
            UniqueID = Guid.NewGuid().ToString();
        }

    }

    class UploadInventory
    {
        static readonly HttpClient client = new HttpClient();
        private static Dictionary<string, string> setNameMap = new Dictionary<string, string>();
        private static readonly string UNKNOWN_SET = "unk";

        private static async Task<SetResponse> LoadSetInformation()
        {
            Console.Write("Reading Set information from Scryfall...");

            // Some sets' names don't match between Deckbox and Scryfall, this allows me to map Deckbox names to Scryfall SetCodes
            Dictionary<string, string> replacementNames = new Dictionary<string, string>()
            {
                // { "Scryfall Name", "Deckbox Name"},
                { "Magic 2014", "Magic 2014 Core Set" },
                { "Magic 2015", "Magic 2015 Core Set" },
                { "Modern Masters 2017", "Modern Masters 2017 Edition" },
            };

            try
            {
                string data = await client.GetStringAsync("https://api.scryfall.com/sets");

                SetResponse response = JsonConvert.DeserializeObject<SetResponse>(data);

                List<SetData> setList = response.data;
                foreach (SetData curSet in setList)
                {
                    string setName = (replacementNames.ContainsKey(curSet.Name) ? replacementNames[curSet.Name] : curSet.Name);
                    setNameMap.Add(setName, curSet.Code);
                }

                Console.WriteLine("Got {0} sets", setList.Count);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: {0}", ex.Message);
            }

            return null;
        }

        private static List<MTG_Card> ReadCardsFromFile(string filename)
        {
            Console.Write("Reading from CSV file ({0})...", filename);

            List<MTG_Card> res = new List<MTG_Card>();

            using (var reader = new StreamReader(filename))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                // Supposedly, this can be done with a specific record class instead of dynamic
                //     I haven't been able to get it to work, but this isn't too bad
                var results = csv.GetRecords<dynamic>();

                foreach (dynamic record in results)
                {
                    IDictionary<String, Object> card_props = (IDictionary<String, Object>)record;

                    int count = 1, number = 0; // Default values in case the CSV is missing data
                    int.TryParse(card_props["Count"].ToString(), out count);
                    int.TryParse(card_props["Card Number"].ToString(), out number);

                    // Use a fake set code for cards that don't fit into another set
                    string set = card_props["Edition"]?.ToString();
                    string setCode = (set != null & setNameMap.ContainsKey(set) ? setNameMap[set] : UNKNOWN_SET);

                    MTG_Card card = new MTG_Card
                    {
                        Count = count,
                        Collector_Number = number,
                        Name = card_props["Name"].ToString(),
                        Set = set,
                        SetCode = setCode,
                        Foil = card_props["Foil"].ToString().ToLower().Equals("foil"),
                    };

                    res.Add(card);
                }

                Console.WriteLine("Number of records read: {0}", res.Count);
            }

            return res;
        }

        private static async Task UploadCardsToFirebase(List<MTG_Card> theList)
        {
            System.Console.WriteLine("Uploading to database...");
            FirestoreDb db = FirestoreDb.Create("mtg-inventory-9d4ca");

            int count = 0;
            foreach (MTG_Card curCard in theList)
            {
                DocumentReference docRef = db.Collection("user_inventory").Document(curCard.UniqueID);
                Dictionary<string, object> entry = new Dictionary<string, object>
                {
                    // These fields should be set for every card
                    { "Name", curCard.Name },
                    { "Set_Code", curCard.SetCode },
                    { "Collector_Number", curCard.Collector_Number },
                    { "Count", curCard.Count }
                };

                // These fields are only set if needed
                if (curCard.Foil) entry.Add("Foil", true);
                if (curCard.SetCode.Equals(UNKNOWN_SET)) entry.Add("Set", curCard.Set);

                await docRef.SetAsync(entry);

                count++;
                if (count % 100 == 0)
                    Console.WriteLine("Cards written: {0}", count);
            }

            Console.WriteLine("Done");
        }
        private static void writeCardsToJson(List<MTG_Card> theList)
        {
            Console.WriteLine("Writing to JSON file");

            string filename = "../web-ui/src/data/hard_coded.js";

            List<string> lines = new List<string>();
            Boolean needcomma = false;
            lines.Add("export const hard_coded_inventory = [");
            foreach (MTG_Card card in theList)
            {
                lines.Add((needcomma ? "," : "") + JsonConvert.SerializeObject(card));
                needcomma = true;
            }
            lines.Add("];");
            File.WriteAllLines(filename, lines);

            // string result = JsonConvert.SerializeObject(theList);
            // await File.WriteAllTextAsync(filename, result);

            Console.WriteLine("Done");
        }

        public static async Task Main(string[] args)
        {
            await LoadSetInformation();

            List<MTG_Card> theList = ReadCardsFromFile("Inventory.csv");
            // await UploadCardsToFirebase(theList);
            writeCardsToJson(theList);
        }

    }
}