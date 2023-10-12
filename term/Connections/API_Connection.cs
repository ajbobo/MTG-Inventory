using Newtonsoft.Json.Linq;
using ExtensionMethods;
using System.Configuration;
using System.Text;

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

        async public Task<List<CardData>> GetCardsInSet(string targetSetCode, FilterSettings filtersettings)
        {
            List<CardData> results = new();

            StringBuilder urlParams = new();
            string colorFilter = filtersettings.GetColorsAPI();
            string rarityFilter = filtersettings.GetRaritiesAPI();
            string countFilter = filtersettings.GetCountAPI();
            AddParam("color", colorFilter, urlParams);
            AddParam("rarity", rarityFilter, urlParams);
            AddParam("count", countFilter, urlParams);

            string url = ConfigurationManager.AppSettings["GetCollection_Url"]! + "?" + urlParams.ToString();
            HttpResponseMessage msg = await _httpClient.GetAsync(string.Format(url, targetSetCode));
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
                    cardData.Add("color", cardDefinition["colorIdentity"].AsString());
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

        private void AddParam(string name, string val, StringBuilder paramList)
        {
            if (val == null || val.Length == 0)
                return;
                
            if (paramList.Length > 0)
                paramList.Append("&");
            paramList.Append(name);
            paramList.Append("=");
            paramList.Append(val);
        }
    }
}