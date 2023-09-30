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
        builder.Services.AddDbContext<MtgInvContext>(opt => opt.UseCosmos(
            "AccountEndpoint=https://mtg-inventory.documents.azure.com:443/;AccountKey=fmo4nUmFhIUotZFeq6v3TZGQhGg3VsZRGMmQqreeB6di4ICxVJovNeqCdkkQOLivFmO6YfuwTPArACDbXjrVjA==;",
            databaseName: "MTG-Inventory"
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