using Newtonsoft.Json;

namespace MTG_CLI
{
    class CLI_Window
    {
        private static HttpClient httpClient = new HttpClient();

        private static Inventory _inventory = new();
        private static List<Scryfall.Set> _setList = new();
        private static List<Scryfall.Card> _cardList = new();

        async private static Task<bool> GetSetData()
        {
            _setList.Clear();

            HttpResponseMessage msg = await httpClient.GetAsync("https://api.scryfall.com/sets");
            if (msg.IsSuccessStatusCode)
            {
                string respStr = await msg.Content.ReadAsStringAsync();
                Scryfall.SetListResponse resp = JsonConvert.DeserializeObject<Scryfall.SetListResponse>(respStr) ?? new();
                foreach (Scryfall.Set curSet in resp.Data)
                {
                    if (curSet.Set_Type == Scryfall.Set.SetType.CORE || 
                        curSet.Set_Type == Scryfall.Set.SetType.EXPANSION ||
                        curSet.Set_Type == Scryfall.Set.SetType.MASTERPIECE ||
                        curSet.Set_Type == Scryfall.Set.SetType.MASTERS ||
                        curSet.Set_Type == Scryfall.Set.SetType.COMMANDER ||
                        // To limit the number of funny sets to ones that are (mostly) actually collectable, I needed to add some more filters
                        (curSet.Set_Type == Scryfall.Set.SetType.FUNNY && curSet.Block_Code.Length == 0 && curSet.Parent_Set_Code.Length == 0))
                        _setList.Add(curSet);
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
                HttpResponseMessage msg = await httpClient.GetAsync($"https://api.scryfall.com/cards/search?q=set:{targetSet.Code} and game:paper&order=set&unique=prints&page={page}");
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
            var win = new TerminalView(_inventory);

            win.SetList = _setList;
            win.SelectedSetChanged += async (newSet) =>
            {
                win.SetCurrentSet(newSet);
                await GetSetCards(newSet);
                win.SetCardList(_cardList);
            };

            win.Start();
        }

        async public static Task Main(string[] args)
        {
            Console.Title = "Inventory Terminal";
            Console.WriteLine("Reading Set data from Scryfall");
            await GetSetData();

            Console.WriteLine("Reading Inventory data");
            await _inventory.ReadData();

            StartTerminalView();
        }
    }
}