using Terminal.Gui;
using Newtonsoft.Json.Linq;

namespace MTG_CLI
{
    class CLI_Window
    {
        private static HttpClient httpClient = new HttpClient();

        private static List<Scryfall.Set> SetList = new List<Scryfall.Set>();
        private static Scryfall.Set? SelectedSet;

        private static MenuBar CreateMenu()
        {
            var menu = new MenuBar(new MenuBarItem[] {
                new MenuBarItem("_File", new MenuItem[] {
                    new MenuItem("E_xit", "", () => Application.RequestStop() )
                })
            });

            return menu;
        }

        private static Window CreateMainWindow()
        {
            var win = new Window("Magic: the Gathering -- Personal Inventory")
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };

            return win;
        }

        private static void AddCommandLabels(Window win)
        {
            win.Add(new Label("Ctrl-F: Change Filters")
            {
                X = 0,
                Y = 0
            });
            win.Add(new Label("Ctrl-S: Select Set")
            {
                X = 0,
                Y = 1
            });
        }

        private static FrameView CreateSetListSelection()
        {
            // TODO: This should probably be a modal dialog instead of an imbedded frame
            var frame = new FrameView("Select a Set")
            {
                X = 0,
                Y = 2,
                Width = Dim.Percent(50),
                Height = 10
            };
            var listView = new ListView(SetList)
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };
            frame.Add(listView);

            return frame;
        }

        async private static Task<bool> GetStartingData()
        {
            // TODO: I kind of want to move the data storage (Sets, Inventory, maybe more) out of the CLI_Window class
            HttpResponseMessage msg = await httpClient.GetAsync("https://api.scryfall.com/sets");
            if (msg.IsSuccessStatusCode)
            {
                string resp = await msg.Content.ReadAsStringAsync();
                JObject respObj = JObject.Parse(resp);
                IList<JToken> data = respObj["data"]?.Children().ToList<JToken>() ?? new List<JToken>();
                foreach (JToken token in data)
                {
                    Scryfall.Set? curSet = token.ToObject<Scryfall.Set>() ?? Scryfall.Set.NONE;
                    if (curSet.Set_Type == Scryfall.SetType.CORE || curSet.Set_Type == Scryfall.SetType.EXPANSION)
                        SetList.Add(curSet);
                }
                SelectedSet = SetList[0];
                return true;
            }
            else
            {
                // Do something smart here
            }
            return false;
        }

        async public static Task Main(string[] args)
        {
            await GetStartingData();

            Application.Init();

            var menu = CreateMenu();
            var win = CreateMainWindow();

            AddCommandLabels(win);

            var selectSet = CreateSetListSelection();
            win.Add(selectSet);

            Application.Top.Add(menu, win);

            Application.Run();
        }
    }
}