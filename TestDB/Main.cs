using System;
using System.Net.Http;
using Newtonsoft.Json;

namespace TestDB
{
    public class Migrator
    {
        private static HttpClient httpClient = new();

        public class OldData
        {
            public Dictionary<string, Dictionary<string, MTG_Card>> Data { get; set; } = new();
        }

        public static OldData GetJsonData()
        {
            Console.WriteLine("Getting Old Data from JSON file");

            const string CACHE_FILE = "inventory_cache.json";
            using (StreamReader reader = new StreamReader(CACHE_FILE))
            {
                string json = reader.ReadToEnd();
                OldData data = new OldData();
                data.Data = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, MTG_Card>>>(json) ?? new();
                return data;
            }
        }

        public async static Task<List<Inv_Set>> CreateInvData(OldData old)
        {
            List<Inv_Set> inv_SetList = new();
            foreach (string curCode in old.Data.Keys)
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

        private static void CopyCardCounts(Inv_Set inv_set, OldData old)
        {
            Dictionary<string, MTG_Card> oldCards = old.Data[inv_set.Code];
            foreach (Inv_Card card in inv_set.Cards)
            {
                if (oldCards.ContainsKey(card.CollectorNumber))
                {
                    MTG_Card oldCard = oldCards[card.CollectorNumber];
                    card.Counts.Clear();
                    foreach (CardTypeCount ctc in oldCard.Counts)
                    {
                        string attrs = ctc.GetAttrs();
                        int count = ctc.Count;
                        card.Counts.Add(new Inv_CardTypeCount() { Attrs = attrs, Count = count });
                    }
                }
            }
        }

        public async static Task<Inv_Set> PopulateInvCards(Scryfall.Set curSet)
        {
            // Get all the cards from Scryfall to start the set
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

        public static void WriteInv(List<Inv_Set> inventory)
        {
            JsonSerializerSettings settings = new();
            settings.Formatting = Formatting.Indented;

            File.WriteAllText("updated_inventory.json", JsonConvert.SerializeObject(inventory, settings));
        }

        public async static Task Main()
        {
            OldData old = GetJsonData();

            List<Inv_Set> inv = await CreateInvData(old);

            WriteInv(inv);
        }
    }
}