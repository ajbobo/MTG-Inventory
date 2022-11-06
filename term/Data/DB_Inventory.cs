using System.Configuration;

namespace MTG_CLI
{
    public class DB_Inventory : IDB_Inventory
    {
        readonly private string _cardNumber = ConfigurationManager.AppSettings["Card_Field_Number"] ?? "";
        readonly private string _cardName = ConfigurationManager.AppSettings["Card_Field_Name"] ?? "";
        readonly private string _cardCounts = ConfigurationManager.AppSettings["Card_Field_Counts"] ?? "";
        readonly private string _dbSetCode = ConfigurationManager.AppSettings["DB_Card_Field_SetCode"] ?? "";
        readonly private string _dbNumber = ConfigurationManager.AppSettings["DB_Card_Field_Number"] ?? "";
        readonly private string _dbName = ConfigurationManager.AppSettings["DB_Card_Field_Name"] ?? "";
        readonly private string _dbAttrs = ConfigurationManager.AppSettings["DB_Card_Field_Attrs"] ?? "";
        readonly private string _dbCount = ConfigurationManager.AppSettings["DB_Card_Field_Count"] ?? "";
        
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
                string collectorNumber = curCard[_cardNumber].ToString() ?? "0";
                string name = curCard[_cardName].ToString() ?? "";
                Dictionary<string, object> counts = (Dictionary<string, object>)curCard[_cardCounts]; // This is really <string, long>
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
                setCode = _sql.ReadValue<string>(_dbSetCode, ""); // This shouldn't change, but we'll set it here anyway

                string collectorNumber = _sql.ReadValue<string>(_dbNumber, "");
                string name = _sql.ReadValue<string>(_dbName, "");
                string attrs = _sql.ReadValue<string>(_dbAttrs, "");
                int count = _sql.ReadValue<int>(_dbCount, 0);

                if (!lastCollectorNumber.Equals(collectorNumber)) // We're at a new card in the table, make a new one and add it to the list
                {
                    curCard = new();
                    fullSet.Add(curCard);

                    curCard.Add(_cardNumber, collectorNumber);
                    curCard.Add(_cardName, name);
                    curCard.Add(_cardCounts, new Dictionary<string, int> { { attrs, count } });
                }
                else if (!lastAttrs.Equals(attrs)) // New CTC - add it to the last card
                {
                    Dictionary<string, int> ctcs = (Dictionary<string, int>)curCard[_cardCounts];
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