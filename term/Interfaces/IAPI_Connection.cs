namespace MTG_CLI
{
    public interface IAPI_Connection
    {
        Task<List<string>> GetCollectableSets();
        Task<List<XCardData>> GetCardsInSet(string targetSetCode, FilterSettings filterSettings);
        Task<List<XCardData>> GetCardsInSet(string targetSetCode);
    }
}