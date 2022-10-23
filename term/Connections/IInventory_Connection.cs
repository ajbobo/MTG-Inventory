namespace MTG_CLI
{
    public interface IInventory_Connection
    {
        Task ReadData(string setCode);
        Task ReadFromFirebase(string setCode);
        Task WriteToFirebase();
    }
}