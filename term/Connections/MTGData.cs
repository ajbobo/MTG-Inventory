using Newtonsoft.Json.Linq;
using ExtensionMethods;
using static MTG_CLI.SQLManager.InternalQuery;

namespace MTG_CLI
{
    public class MTG_Connection
    {
        private SQLManager _sql;
        private HttpClient _httpClient;

        public MTG_Connection(SQLManager sql, HttpClient httpClient)
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
            _sql.Query(CREATE_SET_TABLE).Execute();

            HttpResponseMessage msg = await _httpClient.GetAsync("https://api.scryfall.com/sets");
            if (msg.IsSuccessStatusCode)
            {
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
                        _sql.Query(INSERT_SET)
                            .WithParam("@SetCode", curSet["code"].AsString())
                            .WithParam("@Name", curSet["name"].AsString())
                            .Execute();
                    }
                }
                return true;
            }
            return false;
        }

        async public Task<bool> GetSetCards(string targetSetCode)
        {
            _sql.Query(CREATE_CARD_TABLE).Execute();

            int page = 1;
            bool done = false;
            while (!done)
            {
                HttpResponseMessage msg = await _httpClient.GetAsync($"https://api.scryfall.com/cards/search?q=set:{targetSetCode} and game:paper&order=set&unique=prints&page={page}");
                if (msg.IsSuccessStatusCode)
                {
                    string respStr = await msg.Content.ReadAsStringAsync();
                    JObject resp = JObject.Parse(respStr);
                    JEnumerable<JToken> data = resp["data"]?.Children() ?? new();
                    foreach (JToken curCard in data)
                    {
                        _sql.Query(INSERT_CARD)
                            .WithParam("@SetCode", curCard["set"].AsString())
                            .WithParam("@CollectorNumber", curCard["collector_number"].AsString()) // Scryfall uses "collector_number", but I don't want the underscore anywhere else
                            .WithParam("@Name", curCard["name"].AsString())
                            .WithParam("@Rarity", curCard["rarity"].AsString())
                            .WithParam("@ColorIdentity", curCard["color_identity"].CompressArray())
                            .WithParam("@ManaCost", curCard["mana_cost"].AsString())
                            .WithParam("@TypeLine", curCard["type_line"].AsString());

                        if (curCard["oracle_text"] != null)
                        {
                            _sql.WithParam("@FrontText", curCard["oracle_text"].AsString());
                        }
                        else if (curCard["card_faces"] != null)
                        {
                            _sql.WithParam("@FrontText", curCard["card_faces"]?[0]?["oracle_text"].AsString() ?? "");
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