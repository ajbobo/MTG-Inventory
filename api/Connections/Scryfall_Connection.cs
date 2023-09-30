using Newtonsoft.Json.Linq;
using ExtensionMethods;

namespace mtg_api;

public class Scryfall_Connection : IScryfall_Connection
{
    private readonly HttpClient _httpClient;

    public Scryfall_Connection(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public bool IsCollectableSetType(string setType, string block, string parent)
    {
        return setType.Equals("core") ||
                setType.Equals("expansion") ||
                setType.Equals("masterpiece") ||
                setType.Equals("masters") ||
                setType.Equals("commander") ||
                setType.Equals("draft_innovation") ||
                // To limit the number of funny sets to ones that are (mostly) actually collectable, I needed to add some more filters
                (setType.Equals("funny") && block.Length == 0 && parent.Length == 0);
    }

    async public Task<List<MTG_Set>> GetCollectableSets()
    {
        var setList = new List<MTG_Set>();

        HttpResponseMessage msg = await _httpClient.GetAsync(System.Configuration.ConfigurationManager.AppSettings["GetSetList_Url"]);
        if (!msg.IsSuccessStatusCode)
            return setList;

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
                var set = new MTG_Set
                {
                    SetCode = curSet["code"].AsString(),
                    SetName = curSet["name"].AsString()
                };
                setList.Add(set);
            }
        }
        return setList;
    }

    async public Task<List<MTG_Card>> GetCardsInSet(string targetSetCode)
    {
        int page = 1;
        List<MTG_Card> cardList = new List<MTG_Card>();
        bool done = false;
        while (!done)
        {
            HttpResponseMessage msg = await _httpClient.GetAsync(string.Format(System.Configuration.ConfigurationManager.AppSettings["SetSearch_Url"]!, targetSetCode, page));
            if (msg.IsSuccessStatusCode)
            {
                string respStr = await msg.Content.ReadAsStringAsync();
                JObject resp = JObject.Parse(respStr);
                JEnumerable<JToken> data = resp["data"]!.Children();
                foreach (JToken curCard in data)
                {
                    var card = new MTG_Card();
                    JToken prices = curCard["prices"] ?? new JObject();

                    card.SetCode = curCard["set"].AsString();

                    card.CollectorNumber = curCard["collector_number"].AsString(); // Scryfall uses "collector_number", but I don't want the underscore anywhere else
                    card.Name = curCard["name"].AsString();
                    card.Rarity = curCard["rarity"].AsString();
                    card.ColorIdentity = curCard["color_identity"].CompressArray();
                    card.TypeLine = curCard["type_line"].AsString();
                    // TODO: Figure out how to convert this to a Decimal
                    // card.Price = prices["usd"].AsString()) 
                    // card.PriceFoil = (prices["usd_foil"].HasValue() ? prices["usd_foil"].AsString() : prices["usd_etched"].AsString()));

                    if (curCard["oracle_text"] != null)
                    {
                        card.FrontText = curCard["oracle_text"].AsString();
                        card.CastingCost = curCard["mana_cost"].AsString();
                    }
                    else if (curCard["card_faces"] != null)
                    {
                        card.FrontText = curCard["card_faces"]![0]!["oracle_text"].AsString();
                        card.CastingCost = curCard["card_faces"]![0]!["mana_cost"].AsString();
                    }

                    cardList.Add(card);
                }

                if (resp["has_more"]?.Value<bool>() ?? false)
                    page++;
                else
                    done = true;
            }
            else
            {
                // TODO: Do something smart
                return cardList;
            }
        }

        return cardList;
    }
}