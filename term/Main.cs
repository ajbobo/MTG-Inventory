using System.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MTG_CLI
{
    class CLI_Window
    {
        readonly private static string _sqliteFile = ConfigurationManager.ConnectionStrings["SQLite_File"].ConnectionString;

        private static void StartTerminalView(ISQL_Connection sql, IScryfall_Connection mtgData, IFirestore_Connection firestore)
        {
            var win = new TerminalView(sql);

            win.SelectedSetChanged += async (newSet) =>
            {
                win.SetCurrentSet(newSet);

                Console.WriteLine("Getting cards for set: {0}", newSet);
                await mtgData.GetCardsInSet(newSet);

                Console.WriteLine("Getting inventory for {0}", newSet);
                await firestore.ReadData(newSet);

                win.SetCardList();
            };

            win.DataChanged += async () =>
            {
                Console.WriteLine("Writing current inventory to Firebase");
                await firestore.WriteData();
            };

            win.Start();
        }

        private static IHostBuilder CreateHostBuilder()
        {
            return Host.CreateDefaultBuilder()
                .ConfigureServices((_, services) =>
                {
                    services
                        .AddSingleton<ISQL_Connection>(x => ActivatorUtilities.CreateInstance<SQLite_Connection>(x, _sqliteFile))
                        .AddSingleton<IScryfall_Connection, Scryfall_Connection>()
                        .AddSingleton<IFirestore_Connection, Firestore_Connection>()
                        .AddSingleton<IFirestoreDB_Wrapper, FirestoreDB_Wrapper>()
                        .AddSingleton<IDB_Inventory, DB_Inventory>();
                    services.AddHttpClient<IScryfall_Connection, Scryfall_Connection>();
                });
        }
        
        async public static Task Main(string[] args)
        {
            Console.Title = "Inventory Terminal";

            Console.WriteLine("Registering services with IoC Container");
            using IHost host = CreateHostBuilder().Build();
            host.Start();

            Console.WriteLine("Reading Set data from Scryfall");

            IScryfall_Connection? mtgData = host.Services.GetService<IScryfall_Connection>();
            await (mtgData?.GetCollectableSets() ?? Task.FromResult<bool>(false));

            ISQL_Connection? sql = host.Services.GetService<ISQL_Connection>();
            IFirestore_Connection? firestore = host.Services.GetService<IFirestore_Connection>();

            if (sql == null || mtgData == null || firestore == null)
            {
                Console.WriteLine("Something didn't initialize correctly");
                System.Environment.Exit(1);
            }

            StartTerminalView(sql, mtgData, firestore);
        }
    }
}