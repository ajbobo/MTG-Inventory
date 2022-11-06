namespace MTG_CLI
{
    public interface IDB_Inventory
    {
        public void CreateDBTable();
        public void PopulateDBTable(string setCode, CardData[] data);
        public List<CardData> GetTableData(out string setCode);
    }
}