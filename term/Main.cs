﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ExtensionMethods;
using static MTG_CLI.SQLManager.InternalQuery;

namespace MTG_CLI
{
    class CLI_Window
    {
        private static HttpClient _httpClient = new HttpClient();
        private static SQLManager _sql = new SQLManager();

        private static Inventory _inventory = new(_sql);
        private static List<Scryfall.Card> _cardList = new();

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

        async private static Task<bool> GetSetData()
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

        async private static Task<bool> GetSetCards(string targetSetCode)
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
                            .WithParam("@Collector_Number", curCard["collector_number"].AsString())
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

        private static void StartTerminalView()
        {
            var win = new TerminalView(_inventory, _sql);

            win.SelectedSetChanged += async (newSet) =>
            {
                win.SetCurrentSet(newSet);
                await GetSetCards(newSet);
                win.SetCardList(newSet);
            };

            win.Start();
        }

        async public static Task Main(string[] args)
        {
            Console.Title = "Inventory Terminal";


            Console.WriteLine("Reading Set data from Scryfall");
            await GetSetData();

            // Console.WriteLine("Reading Inventory data");
            // await _inventory.ReadData();

            StartTerminalView();
        }
    }
}