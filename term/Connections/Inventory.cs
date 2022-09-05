using Google.Cloud.Firestore;
using static MTG_CLI.SQLManager.InternalQuery;

namespace MTG_CLI
{
    public class Inventory_Connection
    {
        private FirestoreDb _db;
        private SQLManager _sql;

        public Inventory_Connection(SQLManager sql)
        {
            _db = FirestoreDb.Create("mtg-inventory-9d4ca");
            _sql = sql;
        }

        public async Task ReadData(string setCode)
        {
            await ReadFromFirebase(setCode);
        }

        public async Task ReadFromFirebase(string setCode)
        {
            Console.WriteLine("Firebase data");

            _sql.Query(CREATE_USER_INVENTORY).Execute();

            DocumentSnapshot setDoc = await _db.Collection("User_Inv").Document(setCode).GetSnapshotAsync();
            Dictionary<string, object>[] setData;
            setDoc.TryGetValue<Dictionary<string, object>[]>("Cards", out setData);
            if (setData == null)
                return;

            foreach (Dictionary<string, object> curCard in setData)
            {
                string collectorNumber = curCard["CollectorNumber"].ToString() ?? "0";
                string name = curCard["Name"].ToString() ?? "";
                Dictionary<string, object> counts = (Dictionary<string, object>)curCard["Counts"];
                foreach (string attrs in counts.Keys)
                {
                    long count = (long)counts[attrs];
                    _sql.Query(ADD_TO_USER_INVENTORY)
                        .WithParam("@SetCode", setCode)
                        .WithParam("@CollectorNumber", collectorNumber)
                        .WithParam("@Name", name)
                        .WithParam("@Attrs", attrs)
                        .WithParam("@Count", count)
                        .Execute();
                }
            }
        }

        async public Task WriteToFirebase()
        {
            // Build the data structure for the entire set - We'll send that to Firebase
            List<Dictionary<string, object>> fullSet = new();

            _sql.Query(GET_USER_INVENTORY).Read();

            string setCode = "", lastCollectorNumber = "", lastAttrs = "";
            Dictionary<string, object> curCard = new();
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

            // Write the full set to Firebase
            CollectionReference collection = _db.Collection("User_Inv");
            Dictionary<string, object> cards = new Dictionary<string, object>
            {
                { "Cards", fullSet.ToArray() }
            };
            await collection.Document(setCode).SetAsync(cards);
        }
    }
}