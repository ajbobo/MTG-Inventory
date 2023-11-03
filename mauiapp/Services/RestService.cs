using System.Diagnostics;
using Newtonsoft.Json;

namespace mauiapp;

public class RestService : IRestService
{
    private HttpClient _httpClient;

    public RestService()
    {
        _httpClient = new();
    }


    public async Task<List<CardData>> GetCardsInSet(string setCode)
    {
        var cardList = new List<CardData>();
        if (setCode != null)
        {
            HttpResponseMessage resp = await _httpClient.GetAsync("https://mtg-inventory.azurewebsites.net/api/Collection/" + setCode);
            if (resp.IsSuccessStatusCode)
            {
                var cardStr = await resp.Content.ReadAsStringAsync();
                cardList = JsonConvert.DeserializeObject<List<CardData>>(cardStr);
            }
        }

        return cardList;
    }
}