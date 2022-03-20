using Newtonsoft.Json.Linq;

namespace MTG_CLI
{
    class CLI_Window
    {
        private static HttpClient httpClient = new HttpClient();

        private static List<Scryfall.Set> SetList = new List<Scryfall.Set>();
        private static List<Scryfall.Card> CardList = new List<Scryfall.Card>();

        async private static Task<bool> GetStartingData()
        {
            SetList.Clear();

            HttpResponseMessage msg = await httpClient.GetAsync("https://api.scryfall.com/sets");
            if (msg.IsSuccessStatusCode)
            {
                string respStr = await msg.Content.ReadAsStringAsync();
                JObject respObj = JObject.Parse(respStr);
                Scryfall.SetListResponse resp = respObj.ToObject<Scryfall.SetListResponse>() ?? Scryfall.SetListResponse.NONE;
                foreach (Scryfall.Set curSet in resp.data ?? new List<Scryfall.Set>())
                {
                    if (curSet.Set_Type == Scryfall.Set.SetType.CORE || curSet.Set_Type == Scryfall.Set.SetType.EXPANSION)
                        SetList.Add(curSet);
                }
                return true;
            }
            return false;
        }

        async private static Task<bool> GetSetCards(Scryfall.Set targetSet)
        {
            CardList.Clear();

            int page = 1;
            bool done = false;
            while (!done)
            {
                HttpResponseMessage msg = await httpClient.GetAsync($"https://api.scryfall.com/cards/search?q=set:{targetSet.Code}&order=set&unique=prints&page={page}");
                if (msg.IsSuccessStatusCode)
                {
                    string respStr = await msg.Content.ReadAsStringAsync();
                    JObject respObj = JObject.Parse(respStr);
                    Scryfall.CardListResponse resp = respObj.ToObject<Scryfall.CardListResponse>() ?? Scryfall.CardListResponse.NONE;
                    foreach (Scryfall.Card curCard in resp.data ?? new List<Scryfall.Card>())
                    {
                        CardList.Add(curCard);
                    }
                    if (resp.has_more ?? false)
                        page++;
                    else
                        done = true;
                }
                else {
                    // Do something smart
                    return false;
                }
            }

            return true;
        }

        async public static Task Main(string[] args)
        {
            await GetStartingData();

            var win = new TerminalView();

            // Configure TerminalWindow with callbacks, events, etc.
            win.SetList = SetList;
            win.SelectedSetChanged += async (newSet) =>
            {
                win.SetCurrentSet(newSet);
                await GetSetCards(newSet);
                win.SetCardList(CardList);
            };

            win.Start();
        }
    }
}