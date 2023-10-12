using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MTG_CLI
{
    [ExcludeFromCodeCoverage]
    class CLI_Window
    {
        readonly private static string _sqliteFile = ConfigurationManager.ConnectionStrings["SQLite_File"].ConnectionString;
        // readonly private static string _sqliteFile = ConfigurationManager.ConnectionStrings["SQLite_InMemory"].ConnectionString;

        private static void StartTerminalView(ISQL_Connection sql, IFirestore_Connection firestore, IAPI_Connection api)
        {
            var win = new TerminalView(sql, api);

            win.SelectedSetChanged += (newSet, newName) =>
            {
                win.SetCurrentSet(newSet, newName);
                win.SetCardList(newSet);
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
                        .AddSingleton<IFirestore_Connection, Firestore_Connection>()
                        .AddSingleton<IAPI_Connection, API_Connection>()
                        .AddSingleton<IFirestoreDB_Wrapper, FirestoreDB_Wrapper>()
                        .AddSingleton<IDB_Inventory, DB_Inventory>();
                    services.AddHttpClient<IAPI_Connection, API_Connection>();
                });
        }
        
        public static void Main(string[] args)
        {
            Console.Title = "Inventory Terminal";

            using IHost host = CreateHostBuilder().Build();
            host.Start();

            ISQL_Connection? sql = host.Services.GetService<ISQL_Connection>();
            IFirestore_Connection? firestore = host.Services.GetService<IFirestore_Connection>();
            IAPI_Connection? api = host.Services.GetService<IAPI_Connection>();

            if (sql == null || firestore == null || api == null)
            {
                Console.WriteLine("Something didn't initialize correctly");
                System.Environment.Exit(1);
            }

            StartTerminalView(sql, firestore, api);
        }
    }
}