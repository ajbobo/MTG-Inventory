namespace MTG_CLI
{
    public interface IAPI_Connection
    {
        Task<List<string>> GetCollectableSets();
        Task<List<CardData>> GetCardsInSet(string targetSetCode);
    }
}