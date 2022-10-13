using System.Configuration;

namespace MTG_CLI
{
    class CLI_Window
    {
        readonly private static string _sqliteFile = ConfigurationManager.ConnectionStrings["SQLite_File"].ConnectionString;

        private static HttpClient _httpClient = new HttpClient();
        private static ISQLManager _sql = SQLiteManager.GetInstance(_sqliteFile);

        private static Inventory_Connection _inventory = new(_sql);
        private static MTG_Connection _mtgData = new(_sql, _httpClient);

        private static void StartTerminalView()
        {
            var win = new TerminalView(_sql);

            win.SelectedSetChanged += async (newSet) =>
            {
                win.SetCurrentSet(newSet);

                Console.WriteLine("Getting cards for set: {0}", newSet);
                await _mtgData.GetSetCards(newSet);

                Console.WriteLine("Getting inventory for {0}", newSet);
                await _inventory.ReadData(newSet);

                win.SetCardList();
            };

            win.DataChanged += async () =>
            {
                Console.WriteLine("Writing current inventory to Firebase");
                await _inventory.WriteToFirebase();
            };

            win.Start();
        }

        async public static Task Main(string[] args)
        {
            Console.Title = "Inventory Terminal";

            Console.WriteLine("Reading Set data from Scryfall");
            await _mtgData.GetSetList();

            StartTerminalView();
        }
    }
}