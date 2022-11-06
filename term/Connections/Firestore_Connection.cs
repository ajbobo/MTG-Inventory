using System.Configuration;

namespace MTG_CLI
{
    public class Firestore_Connection : IFirestore_Connection
    {
        readonly private string _dbName = ConfigurationManager.AppSettings["Firestore_DB"] ?? "";
        readonly private string _dbCollection = ConfigurationManager.AppSettings["Firestore_Collection"] ?? "";
        readonly private string _dbCardsField = ConfigurationManager.AppSettings["Firestore_CardsField"] ?? "";

        private IFirestoreDB_Wrapper _firestore;
        private IDB_Inventory _dbInv;

        // This could be called directly, but is being called via dependency injection instead
        public Firestore_Connection(IFirestoreDB_Wrapper firestore, IDB_Inventory dbInv)
        {
            _firestore = firestore;
            _firestore.Connect(_dbName);

            _dbInv = dbInv;
        }

        public async Task ReadData(string setCode)
        {
            _dbInv.CreateDBTable();
            CardData[] setData = await _firestore.GetDocumentField(_dbCollection, setCode, _dbCardsField);
            _dbInv.PopulateDBTable(setCode, setData);
        }

        async public Task WriteData()
        {
            // Build the data structure for the entire set - We'll send that to Firebase
            string setCode;
            List<CardData> fullSet = _dbInv.GetTableData(out setCode);
            await _firestore.WriteDocumentField(_dbCollection, setCode, _dbCardsField, fullSet.ToArray());
        }
    }
}