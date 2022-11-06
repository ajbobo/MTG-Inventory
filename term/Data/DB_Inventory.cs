namespace MTG_CLI
{
    public class DB_Inventory : IDB_Inventory
    {
        private ISQL_Connection _sql;

        public DB_Inventory(ISQL_Connection sql)
        {
            _sql = sql;
        }

        public void CreateDBTable()
        {
            _sql.Query(MTG_Query.CREATE_USER_INVENTORY).Execute();
        }

        public void PopulateDBTable(string setCode, CardData[] data)
        {
            foreach (CardData curCard in data)
            {
                string collectorNumber = curCard["CollectorNumber"].ToString() ?? "0";
                string name = curCard["Name"].ToString() ?? "";
                Dictionary<string, object> counts = (Dictionary<string, object>)curCard["Counts"]; // This is really <string, long>
                foreach (string attrs in counts.Keys)
                {
                    long count = (long)counts[attrs];
                    _sql.Query(MTG_Query.ADD_TO_USER_INVENTORY)
                        .WithParam("@SetCode", setCode)
                        .WithParam("@CollectorNumber", collectorNumber)
                        .WithParam("@Name", name)
                        .WithParam("@Attrs", attrs)
                        .WithParam("@Count", count)
                        .Execute();
                }
            }
        }

        public List<CardData> GetTableData(out string setCode)
        {
            List<CardData> fullSet = new();

            _sql.Query(MTG_Query.GET_USER_INVENTORY).OpenToRead();

            string lastCollectorNumber = "", lastAttrs = "";
            setCode = "";
            CardData curCard = new();
            while (_sql.ReadNext())
            {
                setCode = _sql.ReadValue<string>("SetCode", ""); // This shouldn't change, but we'll set it here anyway

                string collectorNumber = _sql.ReadValue<string>("CollectorNumber", "");
                string name = _sql.ReadValue<string>("Name", "");
                string attrs = _sql.ReadValue<string>("Attrs", "");
                int count = _sql.ReadValue<int>("Count", 0);

                if (!lastCollectorNumber.Equals(collectorNumber)) // We're at a new card in the table, make a new one and add it to the list
                {
                    curCard = new();
                    fullSet.Add(curCard);

                    curCard.Add("CollectorNumber", collectorNumber);
                    curCard.Add("Name", name);
                    curCard.Add("Counts", new Dictionary<string, int> { { attrs, count } });
                }
                else if (!lastAttrs.Equals(attrs)) // New CTC - add it to the last card
                {
                    Dictionary<string, int> ctcs = (Dictionary<string, int>)curCard["Counts"];
                    ctcs.Add(attrs, count);
                }

                lastCollectorNumber = collectorNumber;
                lastAttrs = attrs;
            }
            _sql.Close();

            return fullSet;
        }
    }
}