using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Mime;
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

    public async Task<List<MTG_Set>> GetAllSets()
    {
        var setList = new List<MTG_Set>();
        HttpResponseMessage resp = await _httpClient.GetAsync("https://mtg-inventory.azurewebsites.net/api/Sets");
        if (resp.IsSuccessStatusCode)
        {
            var setStr = await resp.Content.ReadAsStringAsync();
            setList = JsonConvert.DeserializeObject<List<MTG_Set>>(setStr);
        }

        return setList;
    }

    public async Task UpdateCardData(CardData cardData)
    {
        var obj = new { ctcs = cardData.CTCs };
        string json = JsonConvert.SerializeObject(obj);
        var content = new StringContent(json);
        content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

        HttpResponseMessage resp = await _httpClient.PutAsync($"https://mtg-inventory.azurewebsites.net/api/Collection/{cardData.Card.SetCode}/{cardData.Card.CollectorNumber}", content);
    }
}