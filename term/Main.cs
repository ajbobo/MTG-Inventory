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

        private static void StartTerminalView(IAPI_Connection api)
        {
            var win = new TerminalView(api);

            win.SelectedSetChanged += (newSet, newName) =>
            {
                win.SetCurrentSet(newSet, newName);
                win.SetCardList(newSet);
            };

            win.Start();
        }

        private static IHostBuilder CreateHostBuilder()
        {
            return Host.CreateDefaultBuilder()
                .ConfigureServices((_, services) =>
                {
                    services
                        .AddSingleton<IAPI_Connection, API_Connection>();
                    services.AddHttpClient<IAPI_Connection, API_Connection>();
                });
        }
        
        public static void Main(string[] args)
        {
            Console.Title = "Inventory Terminal";

            using IHost host = CreateHostBuilder().Build();
            host.Start();

            IAPI_Connection? api = host.Services.GetService<IAPI_Connection>();

            if (api == null)
            {
                Console.WriteLine("Something didn't initialize correctly");
                System.Environment.Exit(1);
            }

            StartTerminalView(api);
        }
    }
}