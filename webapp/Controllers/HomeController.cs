using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using webapp.Models;
using webapp.Utils;

namespace webapp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IActionResult> Collection(string? setCode)
    {
        // Get the list of available sets
        var httpClient = _httpClientFactory.CreateClient();
        var resp = httpClient.GetAsync("https://mtg-inventory.azurewebsites.net/api/Sets");
        MTG_Set? curSet = null;
        if (resp.Result.IsSuccessStatusCode)
        {
            var setStr = await resp.Result.Content.ReadAsStringAsync();
            var setList = JsonConvert.DeserializeObject<List<MTG_Set>>(setStr);
            ViewData["setList"] = setList;

            if (setCode != null && setList != null)
            {
                foreach (MTG_Set set in setList)
                {
                    if (setCode.Equals(set.Code))
                    {
                        curSet = set;
                        ViewData["setName"] = set.Name;
                        ViewData["setIconUrl"] = set.IconUrl;
                    }
                }
            }
        }

        // Get the cards in the selected set
        var cardList = new List<CardData>();
        if (curSet != null)
        {
            resp = httpClient.GetAsync("https://mtg-inventory.azurewebsites.net/api/Collection/" + curSet.Code);
            if (resp.Result.IsSuccessStatusCode)
            {
                var cardStr = await resp.Result.Content.ReadAsStringAsync();
                cardList = JsonConvert.DeserializeObject<List<CardData>>(cardStr);
            }
        }

        // Get the list of symbols
        var symbolList = new List<MTG_Symbol>();
        resp = httpClient.GetAsync("https://mtg-inventory.azurewebsites.net/api/Symbols");
        if (resp.Result.IsSuccessStatusCode)
        {
            var symbolStr = await resp.Result.Content.ReadAsStringAsync();
            symbolList = JsonConvert.DeserializeObject<List<MTG_Symbol>>(symbolStr);
        }
        SymbolHelper.SetSymbolList(symbolList ?? new());

        return View(cardList);
    }

    public IActionResult Decks()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
