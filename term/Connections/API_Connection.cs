using Newtonsoft.Json.Linq;
using ExtensionMethods;
using System.Configuration;
using System.Text;
using Newtonsoft.Json;
using System.Net.Http.Json;
using System.Net.Http.Headers;

namespace MTG_CLI
{
    public class API_Connection : IAPI_Connection
    {
        private readonly HttpClient _httpClient;

        public API_Connection(HttpClient httpClient)
        {
            _httpClient = httpClient;
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

            string url = ConfigurationManager.AppSettings["GetCollection_Url"]!;
            return await CallCardsAPI(targetSetCode, results, url);
        }

        async public Task<List<CardData>> GetCardsInSet(string targetSetCode, string collectorNumber)
        {
            List<CardData> results = new();

            string url = ConfigurationManager.AppSettings["GetCollection_Url"]! + "?collectorNumber=" + collectorNumber;
            return await CallCardsAPI(targetSetCode, results, url);
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
            return await CallCardsAPI(targetSetCode, results, url);
        }

        private async Task<List<CardData>> CallCardsAPI(string targetSetCode, List<CardData> results, string url)
        {
            HttpResponseMessage msg = await _httpClient.GetAsync(string.Format(url, targetSetCode));
            if (msg.IsSuccessStatusCode)
            {
                string respStr = await msg.Content.ReadAsStringAsync();
                CardData[] cardList = JsonConvert.DeserializeObject<CardData[]>(respStr) ?? Array.Empty<CardData>();

                foreach (CardData card in cardList)
                    results.Add(card);
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

        public async void UpdateCardData(CardData card)
        {
            List<CardTypeCount> ctcs = card.CTCs ?? new();

            var obj = new {ctcs = ctcs};

            string json = JsonConvert.SerializeObject(obj);

            string url = ConfigurationManager.AppSettings["PutCollection_Url"]!;
            StringContent content = new(json);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            await _httpClient.PutAsync(string.Format(url, card.Card!.SetCode, card.Card!.CollectorNumber), content);
        }
    }
}