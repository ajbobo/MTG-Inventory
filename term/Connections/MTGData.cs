using Newtonsoft.Json.Linq;
using ExtensionMethods;
using System.Configuration;

namespace MTG_CLI
{
    public class MTG_Connection
    {
        private ISQLManager _sql;
        private HttpClient _httpClient;

        public MTG_Connection(ISQLManager sql, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _sql = sql;
        }

        private static bool IsCollectableSetType(string setType, string block, string parent)
        {
            return (setType.Equals("core") ||
                    setType.Equals("expansion") ||
                    setType.Equals("masterpiece") ||
                    setType.Equals("masters") ||
                    setType.Equals("commander") ||
                    // To limit the number of funny sets to ones that are (mostly) actually collectable, I needed to add some more filters
                    (setType.Equals("funny") && block.Length == 0 && parent.Length == 0));
        }

        async public Task<bool> GetSetList()
        {
            _sql.Query(InternalQuery.CREATE_SET_TABLE).Execute();

            HttpResponseMessage msg = await _httpClient.GetAsync(ConfigurationManager.AppSettings["GetSetList_Url"]);
            if (!msg.IsSuccessStatusCode)
                return false;

            string respStr = await msg.Content.ReadAsStringAsync();

            // I'm parsing this way so that I don't have to worry about large .NET objects that I won't need later
            JObject resp = JObject.Parse(respStr);
            JEnumerable<JToken> data = resp["data"]?.Children() ?? new();
            foreach (JToken curSet in data)
            {
                string type = curSet["set_type"].AsString();
                string block = curSet["block_code"].AsString();
                string parent = curSet["parent_set_code"].AsString();
                if (IsCollectableSetType(type, block, parent))
                {
                    _sql.Query(InternalQuery.INSERT_SET)
                        .WithParam("@SetCode", curSet["code"].AsString())
                        .WithParam("@Name", curSet["name"].AsString())
                        .Execute();
                }
            }
            return true;
        }

        async public Task<bool> GetSetCards(string targetSetCode)
        {
            _sql.Query(InternalQuery.CREATE_CARD_TABLE).Execute();

            int page = 1;
            bool done = false;
            while (!done)
            {
                HttpResponseMessage msg = await _httpClient.GetAsync(string.Format(ConfigurationManager.AppSettings["SetSearch_Url"] ?? "", targetSetCode, page));
                if (msg.IsSuccessStatusCode)
                {
                    string respStr = await msg.Content.ReadAsStringAsync();
                    JObject resp = JObject.Parse(respStr);
                    JEnumerable<JToken> data = resp["data"]?.Children() ?? new();
                    foreach (JToken curCard in data)
                    {
                        JToken prices = curCard["prices"] ?? new JObject();

                        _sql.Query(InternalQuery.INSERT_CARD)
                            .WithParam("@SetCode", curCard["set"].AsString())
                            .WithParam("@CollectorNumber", curCard["collector_number"].AsString()) // Scryfall uses "collector_number", but I don't want the underscore anywhere else
                            .WithParam("@Name", curCard["name"].AsString())
                            .WithParam("@Rarity", curCard["rarity"].AsString())
                            .WithParam("@ColorIdentity", curCard["color_identity"].CompressArray())
                            .WithParam("@TypeLine", curCard["type_line"].AsString())
                            .WithParam("@Price", prices["usd"].AsString())
                            .WithParam("@PriceFoil", (prices["usd_foil"].HasValue() ? prices["usd_foil"].AsString() : prices["usd_etched"].AsString()));

                        if (curCard["oracle_text"] != null)
                        {
                            _sql.WithParam("@FrontText", curCard["oracle_text"].AsString());
                            _sql.WithParam("@ManaCost", curCard["mana_cost"].AsString());
                        }
                        else if (curCard["card_faces"] != null)
                        {
                            _sql.WithParam("@FrontText", curCard["card_faces"]?[0]?["oracle_text"].AsString() ?? "");
                            _sql.WithParam("@ManaCost", curCard["card_faces"]?[0]?["mana_cost"].AsString() ?? "");
                        }


                        _sql.Execute();
                    }

                    if (resp["has_more"]?.Value<bool>() ?? false)
                        page++;
                    else
                        done = true;
                }
                else
                {
                    // Do something smart
                    return false;
                }
            }

            return true;
        }
    }
}