using System;
using System.Net.Http;
using Google.Cloud.Firestore;
using Newtonsoft.Json;

namespace Migrator
{
    public class Migrator
    {
        private static HttpClient httpClient = new();

        public class OldData
        {
            public Dictionary<string, Dictionary<string, Json_Card>> Data { get; set; } = new();
        }

        public static OldData GetJsonData()
        {
            Console.WriteLine("Getting Old Data from JSON file");

            const string CACHE_FILE = "inventory_cache.json";
            using (StreamReader reader = new StreamReader(CACHE_FILE))
            {
                string json = reader.ReadToEnd();
                OldData data = new OldData();
                data.Data = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Json_Card>>>(json) ?? new();
                return data;
            }
        }

        public static Dictionary<string, List<Inv_Card>> ConvertOldData(OldData old)
        {
            Dictionary<string, List<Inv_Card>> inv = new();

            foreach (string setCode in old.Data.Keys)
            {
                List<Inv_Card> newList = new();
                inv.Add(setCode, newList);

                Dictionary<string, Json_Card> curSet = old.Data[setCode];
                foreach (Json_Card card in curSet.Values)
                {
                    newList.Add(new Inv_Card(card));
                }
            }

            return inv;
        }

        public async static Task WriteInventory_Firebase(Dictionary<string, List<Inv_Card>> inventory)
        {
            Console.WriteLine("Writing Inventory to Firestore DB");
            FirestoreDb db = FirestoreDb.Create("testdb-8448b");

            CollectionReference topLevel = db.Collection("User_Inv_2");
            foreach (string setCode in inventory.Keys)
            {
                Console.WriteLine($"Writing set {setCode}");
                DocumentReference set = topLevel.Document(setCode);
                Dictionary<string, List<Inv_Card>> cards = new();
                cards.Add("Cards", inventory[setCode]);
                await set.SetAsync(cards);
            }
        }

        public async static Task Main()
        {
            // This migrates the data to a format that is one Document per set
            // Only inventory data is stored as properties of the document
            // The idea is to import it into an in-memory database (SQLlite, etc) along with Scryfall data
            //    Then the data is queried/filtered in-memory

            OldData old = GetJsonData();
            Dictionary<string, List<Inv_Card>> inventory = ConvertOldData(old);

            try
            {
                await WriteInventory_Firebase(inventory);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}