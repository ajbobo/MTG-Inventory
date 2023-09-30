using System.Runtime.Caching;
using Microsoft.EntityFrameworkCore;

namespace mtg_api;

public class Program
{
    public static void Main(string[] args)
    {
        var cache = MemoryCache.Default;

        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddSingleton(cache);
        builder.Services.AddHttpClient<IScryfall_Connection, Scryfall_Connection>();

        builder.Services.AddControllers();

        string endpoint = System.Configuration.ConfigurationManager.AppSettings["DB_Endpoint"] ?? "";
        string key = System.Configuration.ConfigurationManager.AppSettings["DB_Key"] ?? "";
        string dbName = System.Configuration.ConfigurationManager.AppSettings["DB_Name"] ?? "";
        builder.Services.AddDbContext<MtgInvContext>(opt => opt.UseCosmos(
            $"AccountEndpoint={endpoint};AccountKey={key};",
            databaseName: dbName
            ));
            
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}