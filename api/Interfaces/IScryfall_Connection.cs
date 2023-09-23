namespace mtg_api;

public interface IScryfall_Connection
{
    Task<List<MTG_Set>> GetCollectableSets();
    Task<bool> GetCardsInSet(string targetSetCode);
}