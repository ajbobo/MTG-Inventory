namespace MTG_CLI
{
    public interface IScryfall_Connection
    {
        Task<bool> GetCollectableSets();
        Task<bool> GetCardsInSet(string targetSetCode);
    }
}