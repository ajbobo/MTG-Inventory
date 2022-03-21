using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using Newtonsoft.Json;
using CsvHelper;
using Scryfall;

namespace MTG_Inventory
{
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

        private static MTG_Card FindOrMakeCard(List<MTG_Card> theList, string name, string setCode, string setName, string collectorNumber)
        {
            foreach (MTG_Card card in theList)
            {
                if (card.CollectorNumber.Equals(collectorNumber) &&
                    card.Name.Equals(name) &&
                    card.Set.Equals(setName) &&
                    card.SetCode.Equals(setCode))
                {
                    return card;
                }
            }

            MTG_Card newCard = new MTG_Card
            {
                CollectorNumber = collectorNumber,
                Name = name,
                Set = setName,
                SetCode = setCode,
            };
            theList.Add(newCard);

            return newCard;
        }

        private static List<MTG_Card> ReadCardsFromFile(string filename)
        {
            Console.WriteLine("Reading from CSV file ({0})...", filename);

            List<MTG_Card> res = new List<MTG_Card>();

            using (var reader = new StreamReader(filename))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var results = csv.GetRecords<dynamic>();

                foreach (dynamic record in results)
                {
                    IDictionary<String, Object> card_props = (IDictionary<String, Object>)record;

                    int count = 1; // Default values in case the CSV is missing data
                    string collectorNumber = "0"; 
                    bool foil = false, prerelease = false, spanish = false;

                    int.TryParse(card_props["Count"].ToString(), out count);
                    collectorNumber = card_props["Card Number"].ToString();
                    foil = (card_props["Foil"].ToString().ToLower().Equals("true"));
                    prerelease = (card_props["PreRelease"].ToString().ToLower().Equals("true"));
                    spanish = (card_props["Language"].ToString().ToLower().Equals("spanish"));

                    // Use a fake set code for cards that don't fit into another set
                    string set = card_props["Edition"]?.ToString();
                    string setCode = (set != null & setNameMap.ContainsKey(set) ? setNameMap[set] : UNKNOWN_SET);

                    MTG_Card card = FindOrMakeCard(res, card_props["Name"].ToString(), setCode, set, collectorNumber);
                    card.SetCount(count, foil, prerelease, spanish);
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
                DocumentReference docRef = await db.Collection("user_inventory").AddAsync(curCard); // Add Async() will create a document with a random name

                count++;
                if (count % 100 == 0)
                {
                    Console.WriteLine("Cards written: {0}", count);
                }
            }

            Console.WriteLine("Done");
        }

        private static void writeCardsToJson(List<MTG_Card> theList)
        {
            Console.WriteLine("Writing to JSON file");

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            string filename = "../web-ui/src/data/hard_coded.js";

            List<string> lines = new List<string>();
            Boolean needcomma = false;
            lines.Add("[");
            // lines.Add("export const hard_coded_inventory = [");
            foreach (MTG_Card card in theList)
            {
                lines.Add((needcomma ? "," : "") + JsonConvert.SerializeObject(card, settings));
                needcomma = true;
            }
            lines.Add("]");
            // lines.Add("];");
            File.WriteAllLines(filename, lines);

            Console.WriteLine("Done");
        }

        public static async Task Main(string[] args)
        {
            await LoadSetInformation();

            List<MTG_Card> theList = ReadCardsFromFile("data/Inventory.csv");
            writeCardsToJson(theList);
            await UploadCardsToFirebase(theList);
        }

    }
}