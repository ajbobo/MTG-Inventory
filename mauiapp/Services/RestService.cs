using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using Newtonsoft.Json;

namespace mauiapp;

public class RestService : IRestService
{
    private HttpClient _httpClient;

    public RestService()
    {
        _httpClient = new();
    }


    public async Task<List<CardData>> GetCardsInSet(string setCode, string countFilter = "", string priceFilter = "", string rarityFilter = "")
    {
        var cardList = new List<CardData>();
        if (setCode != null)
        {
            var builder = new StringBuilder("https://mtg-inventory.azurewebsites.net/api/Collection/");
            builder.Append(setCode);
            builder.Append('?');
            builder.Append(countFilter.Length > 0 ? "&count=" + countFilter : null);
            builder.Append(priceFilter.Length > 0 ? "&price=" + priceFilter : null);
            builder.Append(rarityFilter.Length > 0 ? "&rarity=" + rarityFilter : null);
            HttpResponseMessage resp = await _httpClient.GetAsync(builder.ToString());
            if (resp.IsSuccessStatusCode)
            {
                var cardStr = await resp.Content.ReadAsStringAsync();
                cardList = JsonConvert.DeserializeObject<List<CardData>>(cardStr);
            }
        }
        foreach (CardData card in cardList)
        {
            if (card.CTCs == null)
                card.CTCs = new();
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