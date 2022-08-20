using Google.Cloud.Firestore;
using Newtonsoft.Json;
using static MTG_CLI.SQLManager.InternalQuery;

namespace MTG_CLI
{
    public class Inventory
    {
        private const string CACHE_FILE = "inventory_cache.json";
        private readonly TimeSpan CACHE_TIMEOUT = new(0, 15, 0); // 15 minutes

        private FirestoreDb _db;
        private SQLManager _sql;

        public bool UsingCache { get; private set; }

        // Inventory structure: _inventory[SetCode][CardNum] = Card
        private Dictionary<string, Dictionary<string, MTG_Card>> _inventory = new();

        public Inventory(SQLManager sql)
        {
            _db = FirestoreDb.Create("mtg-inventory-9d4ca");
            _sql = sql;
        }

        public async Task ReadData(string setCode)
        {
            // Check the cache, if it is too old, read from Firebase
            bool readCache = false;
            // if (File.Exists(CACHE_FILE))
            // {
            //     DateTime lastCacheWrite = File.GetLastWriteTime(CACHE_FILE);
            //     TimeSpan diff = DateTime.Now - lastCacheWrite;
            //     Console.WriteLine("Cache Age: {0}", diff);
            //     if (diff < CACHE_TIMEOUT)
            //         readCache = true;
            //     Console.WriteLine("Reading from cache: {0}", readCache);
            // }

            // if (readCache)
            //     ReadFromJsonCache();
            // else
            await ReadFromFirebase(setCode);

            UsingCache = readCache;
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

        public void ReadFromJsonCache()
        {
            // The cache cuts down on reads from Firebase
            // If you write to it while Firebase isn't available, it won't sync
            //    That would be nice, though
            Console.WriteLine("Local Json data");
            using (StreamReader reader = new StreamReader(CACHE_FILE))
            {
                string json = reader.ReadToEnd();
                _inventory = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, MTG_Card>>>(json) ?? new();
            }
        }

        public void WriteToJsonBackup()
        {
            // JsonSerializerSettings settings = new();
            // settings.Formatting = Formatting.Indented;

            // File.WriteAllText("inventory_cache.json", JsonConvert.SerializeObject(_inventory, settings));
        }

        private void AddCardToInventory(MTG_Card curCard)
        {
            // Console.WriteLine($"{curCard.SetCode} {curCard.CollectorNumber} - {curCard.Name}");

            if (!_inventory.ContainsKey(curCard.SetCode))
                _inventory.Add(curCard.SetCode, new());

            _inventory[curCard.SetCode].Add(curCard.CollectorNumber, curCard);
        }

        public void AddCard(Scryfall.Card curCard, CardTypeCount ctc)
        {
            string setCode = curCard.SetCode;

            if (!_inventory.ContainsKey(setCode))
                _inventory.Add(setCode, new());
            else if (_inventory[setCode].ContainsKey(curCard.CollectorNumber)) // The card is in inventory already - no need to add it
                return;

            MTG_Card newCard = new()
            {
                Name = curCard.Name,
                CollectorNumber = curCard.CollectorNumber,
                SetCode = setCode,
                Set = curCard.SetName
            };
            newCard.Counts.Add(ctc);

            _inventory[setCode].Add(curCard.CollectorNumber, newCard);
        }

        public MTG_Card? GetCard(Scryfall.Card card)
        {
            string setCode = card.SetCode;
            string collectorNumber = card.CollectorNumber;

            if (!_inventory.ContainsKey(setCode) || !_inventory[setCode].ContainsKey(collectorNumber))
                return null;

            return _inventory[setCode][collectorNumber];
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