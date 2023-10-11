using Newtonsoft.Json.Linq;
using ExtensionMethods;
using System.Configuration;

namespace MTG_CLI
{
    public class API_Connection : IAPI_Connection
    {
        private readonly ISQL_Connection _sql;
        private readonly HttpClient _httpClient;

        public API_Connection(ISQL_Connection sql, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _sql = sql;
        }

        async public Task<List<string>> GetCollectableSets()
        {
            List<string> SetList = new();

            HttpResponseMessage msg = await _httpClient.GetAsync(ConfigurationManager.AppSettings["GetSetList_Url"]);
            if (!msg.IsSuccessStatusCode)
                return SetList;

            string respStr = await msg.Content.ReadAsStringAsync();

            // I'm parsing this way so that I don't have to worry about large .NET objects that I won't need later
            JArray resp = JArray.Parse(respStr);
            JEnumerable<JToken> data = resp?.Children() ?? new();
            foreach (JToken curSet in data)
            {
                string name = curSet["name"].AsString();
                string code = curSet["code"].AsString();
                SetList.Add(string.Format("{0,-7} {1}", "(" + code + ")", name));
            }

            return SetList;
        }

        async public Task<List<CardData>> GetCardsInSet(string targetSetCode)
        {
            List<CardData> results = new();

            HttpResponseMessage msg = await _httpClient.GetAsync(string.Format(ConfigurationManager.AppSettings["GetCollection_Url"]!, targetSetCode));
            if (msg.IsSuccessStatusCode)
            {
                string respStr = await msg.Content.ReadAsStringAsync();
                JArray resp = JArray.Parse(respStr);
                JEnumerable<JToken> data = resp!.Children();
                foreach (JToken curCard in data)
                {
                    JToken cardDefinition = curCard["card"]!;

                    CardData cardData = new();
                    cardData.Add("collectorNumber", cardDefinition["collectorNumber"].AsString());
                    cardData.Add("name", cardDefinition["name"].AsString());
                    cardData.Add("rarity", cardDefinition["rarity"].AsString());
                    cardData.Add("color", cardDefinition["color_identity"].AsString());
                    cardData.Add("typeLine", cardDefinition["type_line"].AsString());
                    cardData.Add("price", cardDefinition["price"].AsString());
                    cardData.Add("priceFoil", cardDefinition["priceFoil"].AsString());
                    cardData.Add("frontText", cardDefinition["frontText"].AsString());
                    cardData.Add("manaCost", cardDefinition["castingCost"].AsString());
                    cardData.Add("count", curCard["totalCount"].AsString());

                    results.Add(cardData);
                }
            }
            else
            {
                // Do something smart
                return results;
            }

            return results;
        }
    }
}