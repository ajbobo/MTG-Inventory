using System.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MTG_CLI
{
    class CLI_Window
    {
        readonly private static string _sqliteFile = ConfigurationManager.ConnectionStrings["SQLite_File"].ConnectionString;

        private static void StartTerminalView(ISQLManager sql, IMTG_Connection mtgData, IInventory_Connection inventory)
        {
            var win = new TerminalView(sql);

            win.SelectedSetChanged += async (newSet) =>
            {
                win.SetCurrentSet(newSet);

                Console.WriteLine("Getting cards for set: {0}", newSet);
                await mtgData.GetSetCards(newSet);

                Console.WriteLine("Getting inventory for {0}", newSet);
                await inventory.ReadData(newSet);

                win.SetCardList();
            };

            win.DataChanged += async () =>
            {
                Console.WriteLine("Writing current inventory to Firebase");
                await inventory.WriteToFirebase();
            };

            win.Start();
        }

        private static IHostBuilder CreateHostBuilder()
        {
            return Host.CreateDefaultBuilder()
                .ConfigureServices((_, services) =>
                {
                    services
                        .AddSingleton<ISQLManager>(x => ActivatorUtilities.CreateInstance<SQLiteManager>(x, _sqliteFile))
                        .AddSingleton<IMTG_Connection, MTG_Connection>()
                        .AddSingleton<IInventory_Connection, Inventory_Connection>();
                    services.AddHttpClient<IMTG_Connection, MTG_Connection>();
                });
        }
        
        async public static Task Main(string[] args)
        {
            Console.Title = "Inventory Terminal";

            Console.WriteLine("Registering services with IoC Container");
            using IHost host = CreateHostBuilder().Build();
            host.Start();

            Console.WriteLine("Reading Set data from Scryfall");

            IMTG_Connection? mtgData = host.Services.GetService<IMTG_Connection>();
            await (mtgData?.GetSetList() ?? Task.FromResult<bool>(false));

            ISQLManager? sql = host.Services.GetService<ISQLManager>();
            IInventory_Connection? inventory = host.Services.GetService<IInventory_Connection>();

            if (sql == null || mtgData == null || inventory == null)
            {
                Console.WriteLine("Something didn't initialize correctly");
                System.Environment.Exit(1);
            }

            StartTerminalView(sql, mtgData, inventory);
        }
    }
}