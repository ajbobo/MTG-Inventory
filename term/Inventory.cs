using Google.Cloud.Firestore;
using Newtonsoft.Json;
using static MTG_CLI.SQLManager.InternalQuery;

namespace MTG_CLI
{
    public class Inventory
    {
        private FirestoreDb _db;
        private SQLManager _sql;

        public Inventory(SQLManager sql)
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
            setDoc.TryGetValue<Dictionary<string,object>[]>("Cards", out setData);
            if (setData == null)
                return;

            foreach (Dictionary<string, object> curCard in setData)
            {
                string collectorNumber = curCard["CollectorNumber"].ToString() ?? "0";
                string name = curCard["Name"].ToString() ?? "";
                List<object> counts = (List<object>)curCard["Counts"];
                foreach (Dictionary<string, object> curCTC in counts)
                {
                    string attrs = curCTC["Attrs"].ToString() ?? "Standard";
                    long count = (long)curCTC["Count"];
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

        async public Task WriteToFirebase() //MTG_Card card)
        {
            // CollectionReference collection = _db.Collection("user_inventory");
            // if (card.UUID.Length > 0)
            // {
            //     await collection.Document(card.UUID).SetAsync(card);
            // }
            // else
            // {
            //     DocumentReference doc = await collection.AddAsync(card);
            //     card.UUID = doc.Id;
            // }
        }
    }
}