using System;
using System.Net.Http;
using Google.Cloud.Firestore;
using Newtonsoft.Json;

namespace TestDB
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

        public async static Task<List<Inv_Set>> CreateInvData(OldData old)
        {
            return await CreateInvData(old.Data.Keys.ToArray(), old);
        }

        public async static Task<List<Inv_Set>> CreateInvData(string[] sets, OldData old)
        {
            List<Inv_Set> inv_SetList = new();
            foreach (string curCode in sets)
            {
                Console.WriteLine($"Getting Set: {curCode}");

                HttpResponseMessage msg = await httpClient.GetAsync($"https://api.scryfall.com/sets/{curCode}");
                if (msg.IsSuccessStatusCode)
                {
                    string respStr = await msg.Content.ReadAsStringAsync();
                    Scryfall.Set curSet = JsonConvert.DeserializeObject<Scryfall.Set>(respStr) ?? new();
                    Inv_Set inv_set = await PopulateInvCards(curSet);
                    inv_SetList.Add(inv_set);

                    Console.WriteLine($"Copying Card Counts for {curCode}");
                    CopyCardCounts(inv_set, old);
                }
            }

            return inv_SetList;
        }

        public async static Task<Inv_Set> PopulateInvCards(Scryfall.Set curSet)
        {
            Console.WriteLine($"Adding cards to {curSet.Code}");

            Inv_Set inv_set = new(curSet);

            int page = 1;
            bool done = false;
            while (!done)
            {
                HttpResponseMessage msg = await httpClient.GetAsync($"https://api.scryfall.com/cards/search?q=set:{curSet.Code} and game:paper&order=set&unique=prints&page={page}");
                if (msg.IsSuccessStatusCode)
                {
                    string respStr = await msg.Content.ReadAsStringAsync();
                    Scryfall.CardListResponse resp = JsonConvert.DeserializeObject<Scryfall.CardListResponse>(respStr) ?? new();
                    foreach (Scryfall.Card curCard in resp.Data)
                        inv_set.Cards.Add(new Inv_Card(curCard));

                    if (resp.Has_More)
                        page++;
                    else
                        done = true;
                }
                else
                {
                    Console.WriteLine("What happened??");
                }
            }

            return inv_set;
        }

        private static void CopyCardCounts(Inv_Set inv_set, OldData old)
        {
            Dictionary<string, Json_Card> oldCards = old.Data[inv_set.Code];
            foreach (Inv_Card card in inv_set.Cards)
            {
                if (oldCards.ContainsKey(card.CollectorNumber))
                {
                    Json_Card oldCard = oldCards[card.CollectorNumber];
                    card.Counts.Clear();
                    foreach (Json_CardTypeCount ctc in oldCard.Counts)
                    {
                        string attrs = ctc.GetAttrs();
                        int count = ctc.Count;
                        card.Counts.Add(new Inv_CardTypeCount() { Attrs = attrs, Count = count });
                    }
                }
            }
        }

        public static void WriteInv_Json(List<Inv_Set> inventory)
        {
            JsonSerializerSettings settings = new();
            settings.Formatting = Formatting.Indented;

            File.WriteAllText("updated_inventory.json", JsonConvert.SerializeObject(inventory, settings));
        }

        public async static Task WriteInv_Firebase(List<Inv_Set> inventory)
        {
            Console.WriteLine("Writing to Firestore DB");
            FirestoreDb db = FirestoreDb.Create("testdb-8448b");

            int cnt = 0;
            CollectionReference user = db.Collection("User_Inv");
            foreach (Inv_Set curSet in inventory)
            {
                Console.WriteLine($"Writing set {curSet.Code}");
                DocumentReference set = user.Document(curSet.Code.ToUpper());
                await set.SetAsync(curSet);

                CollectionReference cardList = set.Collection("Cards");
                foreach (Inv_Card curCard in curSet.Cards)
                {
                    cnt++;

                    if (cnt % 50 == 0)
                        Console.WriteLine("{0} written", cnt);

                    await cardList.Document(curCard.CollectorNumber).SetAsync(curCard);
                }
            }
        }

        public async static Task Main()
        {
            OldData old = GetJsonData();

            // List<Inv_Set> inv = await CreateInvData(old); // Migrate them all
            List<Inv_Set> inv = await CreateInvData(new string[] { "snc" }, old); // Migrate certain sets

            WriteInv_Json(inv);
            try
            {
                await WriteInv_Firebase(inv);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}