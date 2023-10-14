namespace MTG_CLI
{
    public interface IDB_Inventory
    {
        public void CreateDBTable();
        public void PopulateDBTable(string setCode, XCardData[] data);
        public List<XCardData> GetTableData(out string setCode);
    }
}