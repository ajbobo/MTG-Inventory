namespace MTG_CLI
{
    public interface IFirebase_Connection
    {
        Task ReadData(string setCode);
        Task WriteData();
    }
}