namespace MTG_CLI
{
    public interface IFirestore_Connection
    {
        Task ReadData(string setCode);
        Task WriteData();
    }
}