using Spectre.Console;
using Newtonsoft.Json.Linq;
using Scryfall;

namespace MTG_CLI
{
    public enum Mode
    {
        FILTER,
        SELECT_SET,
        SELECT_CARD,
        DONE,
    }

    class MainWindow
    {
        private static readonly HttpClient httpClient = new HttpClient();

        private static Mode CurMode;
        private static List<Scryfall.Set> SetList = new List<Scryfall.Set>();
        private static Scryfall.Set? SelectedSet = null;

        private static SetSelectionTable SetTable = new SetSelectionTable(SetList);

        private static void ConfigureCommands()
        {
            Commands.MapKeyToFunction(ConsoleKey.F, () => SetMode(Mode.FILTER));
            Commands.MapKeyToFunction(ConsoleKey.S, () => SetMode(Mode.SELECT_SET));
            Commands.MapKeyToFunction(ConsoleKey.Q, () => SetMode(Mode.DONE));
        }

        private static void SetMode(Mode mode)
        {
            CurMode = mode;
            switch (mode)
            {
                case Mode.SELECT_SET:
                    Commands.MapKeyToFunction(ConsoleKey.UpArrow, SetTable.OnUpArrow);
                    Commands.MapKeyToFunction(ConsoleKey.DownArrow, SetTable.OnDownArrow);
                    Commands.MapKeyToFunction(ConsoleKey.PageUp, SetTable.OnPageUp);
                    Commands.MapKeyToFunction(ConsoleKey.PageDown, SetTable.OnPageDown);
                    Commands.MapKeyToFunction(ConsoleKey.Enter, GetSelectedSet);
                    break;
                default:
                    Commands.ClearFunctions(ConsoleKey.UpArrow, 
                                            ConsoleKey.DownArrow, 
                                            ConsoleKey.PageDown, 
                                            ConsoleKey.PageUp, 
                                            ConsoleKey.Enter);
                    SetTable.Reset();
                    break;
            }
        }

        private static void GetSelectedSet()
        {
            SelectedSet = SetTable.GetSelectedSet();
            SetMode(Mode.SELECT_CARD);
        }

        async private static Task<bool> GetStartingData()
        {
            Console.Clear();
            DrawTitleBanner();

            AnsiConsole.WriteLine("Getting set list from Scryfall");
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
                AnsiConsole.WriteLine("[red]Error reading from Scryfall[/]");
                // Do something smart here
            }
            return false;
        }

        private static void MainLoop()
        {
            while (CurMode != Mode.DONE)
            {
                Console.Clear();
                DrawTitleBanner();

                // List available commands
                AnsiConsole.WriteLine("F: Filters");
                AnsiConsole.WriteLine("S: Choose a Set");
                AnsiConsole.WriteLine("Q: Quit");

                // Set up main frame table
                int fullWidth = Console.WindowWidth;
                Table table = new Table();
                table.Width(fullWidth);
                table.HideHeaders();

                table.AddColumn(new TableColumn("").Width(fullWidth / 2));
                table.AddColumn(new TableColumn("").Width(fullWidth / 2));

                if (CurMode == Mode.SELECT_SET)
                {
                    table.AddRow(new Rule("Select a Set"));
                    table.AddRow(SetTable.GetTable((SelectedSet ?? Scryfall.Set.NONE), Console.WindowHeight - 10));
                }
                else
                {
                    AnsiConsole.Write(new Rule(SelectedSet?.Name ?? "<none>"));
                }

                AnsiConsole.Write(table);

                Commands.ExecuteCommand(Console.ReadKey(true).Key);
            }
            Console.Clear();
        }

        private static void DrawTitleBanner()
        {
            AnsiConsole.Write(new Rule("Magic: The Gathering -- Personal Inventory"));
        }

        async public static Task Main(string[] args)
        {
            SetMode(Mode.SELECT_SET);
            ConfigureCommands();
            await GetStartingData();
            MainLoop();
        }
    }
}