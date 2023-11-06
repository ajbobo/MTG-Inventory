namespace mauiapp;

public interface IRestService
{
    Task<List<CardData>> GetCardsInSet(string setCode);
    Task<List<MTG_Set>> GetAllSets();
    Task UpdateCardData(CardData cardData);
}