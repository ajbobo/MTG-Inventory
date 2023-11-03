namespace mauiapp;

public interface IRestService
{
    Task<List<CardData>> GetCardsInSet(string setCode);
}