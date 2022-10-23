namespace MTG_CLI
{
    public interface IMTG_Connection
    {
        Task<bool> GetSetCards(string targetSetCode);
        Task<bool> GetSetList();
    }
}