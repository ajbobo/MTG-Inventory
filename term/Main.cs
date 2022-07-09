using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static MTG_CLI.SQLManager.InternalQuery;

namespace MTG_CLI
{
    class CLI_Window
    {
        private static HttpClient _httpClient = new HttpClient();
        private static SQLManager _sql = new SQLManager();

        private static Inventory _inventory = new(_sql);
        private static List<Scryfall.Set> _setList = new();
        private static List<Scryfall.Card> _cardList = new();

        async private static Task<bool> GetSetData()
        {
            _setList.Clear();
            _sql.Query(CREATE_SET_TABLE).Go();

            HttpResponseMessage msg = await _httpClient.GetAsync("https://api.scryfall.com/sets");
            if (msg.IsSuccessStatusCode)
            {
                string respStr = await msg.Content.ReadAsStringAsync();

                // I'm parsing this way so that I don't have to worry about large .NET objects that I won't need much
                JObject resp = JObject.Parse(respStr);
                IList<JToken> data = resp["data"]?.Children().ToList() ?? new();
                foreach (JToken curSet in data)
                {
                    string setType = curSet["set_type"]?.ToString() ?? "";
                    if (setType.Equals("core") ||
                        setType.Equals("expansion") ||
                        setType.Equals("masterpiece") ||
                        setType.Equals("masters") ||
                        setType.Equals("commander") ||
                        // To limit the number of funny sets to ones that are (mostly) actually collectable, I needed to add some more filters
                        (setType.Equals("funny") && curSet["block_code"]?.ToString().Length == 0 && curSet["parent_set_code"]?.ToString().Length == 0))
                        _sql.Query(INSERT_SET)
                            .WithParam("@SetCode", curSet["code"]?.ToString() ?? "")
                            .WithParam("@Name", curSet["name"]?.ToString() ?? "")
                            .Go();
                }
                return true;
            }
            return false;
        }

        async private static Task<bool> GetSetCards(Scryfall.Set targetSet)
        {
            _cardList.Clear();

            int page = 1;
            bool done = false;
            while (!done)
            {
                HttpResponseMessage msg = await _httpClient.GetAsync($"https://api.scryfall.com/cards/search?q=set:{targetSet.Code} and game:paper&order=set&unique=prints&page={page}");
                if (msg.IsSuccessStatusCode)
                {
                    string respStr = await msg.Content.ReadAsStringAsync();
                    Scryfall.CardListResponse resp = JsonConvert.DeserializeObject<Scryfall.CardListResponse>(respStr) ?? new();
                    foreach (Scryfall.Card curCard in resp.Data)
                        _cardList.Add(curCard);

                    if (resp.Has_More)
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
                // await GetSetCards(newSet);
                // win.SetCardList(_cardList);
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