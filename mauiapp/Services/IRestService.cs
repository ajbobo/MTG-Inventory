namespace mauiapp;

public interface IRestService
{
    Task<List<CardData>> GetCardsInSet(string setCode, string countFilter = "", string priceFilter = "", string rarityFilter = "");
    Task<List<MTG_Set>> GetAllSets();
    Task UpdateCardData(CardData cardData);
}