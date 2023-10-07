namespace mtg_api;

public interface IScryfall_Connection
{
    Task<List<MTG_Set>> GetCollectableSets();
    Task<List<MTG_Card>> GetCardsInSet(string targetSetCode);
    Task<List<MTG_Symbol>> GetSymbols();
}